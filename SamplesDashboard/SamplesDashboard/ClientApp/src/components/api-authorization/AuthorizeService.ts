import { UserManager, WebStorageStateStore, User, Profile, SignoutResponse } from 'oidc-client';
import { QueryParameterNames, ApplicationPaths, ApplicationName } from './ApiAuthorizationConstants';

export class AuthorizeService {
    callbacks: {callback: () => void, subscription: number}[] = [];
    nextSubscriptionId = 0;
    user: User | undefined = undefined;
    userManager: UserManager | undefined = undefined;
    userIsAuthenticated = false;

    // By default pop ups are disabled because they don't work properly on Edge.
    // If you want to enable pop up authentication simply set this flag to false.
    popUpDisabled = true;

    async isAuthenticated(): Promise<boolean> {
        const user = await this.getUser();
        return !!user;
    }

    async getUser(): Promise<Profile | undefined> {
        if (this.user?.profile) {
            return this.user.profile;
        }

        await this.ensureUserManagerInitialized();
        const user = await this.userManager?.getUser();
        return user?.profile;
    }

    async getAccessToken(): Promise<string | undefined> {
        await this.ensureUserManagerInitialized();
        const user = await this.userManager?.getUser();
        return user?.access_token;
    }

    // We try to authenticate the user in three different ways:
    // 1) We try to see if we can authenticate the user silently. This happens
    //    when the user is already logged in on the IdP and is done using a hidden iframe
    //    on the client.
    // 2) We try to authenticate the user using a PopUp Window. This might fail if there is a
    //    Pop-Up blocker or the user has disabled PopUps.
    // 3) If the two methods above fail, we redirect the browser to the IdP to perform a traditional
    //    redirect flow.
    async signIn(state: {returnUrl: string}): Promise<{status: string, message?: string}> {
        await this.ensureUserManagerInitialized();
        try {
            const silentUser = await this.userManager?.signinSilent(this.createArguments(undefined));
            this.updateState(silentUser);
            return this.success(state);
        } catch (silentError) {
            // User might not be authenticated, fallback to popup authentication
            // tslint:disable-next-line:no-console
            console.log('Silent authentication error: ', silentError);

            try {
                if (this.popUpDisabled) {
                    throw new Error('Popup disabled. Change \'AuthorizeService.js:AuthorizeService.popupDisabled\' to false to enable it.');
                }

                const popUpUser = await this.userManager?.signinPopup(this.createArguments(undefined));
                this.updateState(popUpUser);
                return this.success(state);
            } catch (popUpError) {
                if (popUpError.message === 'Popup window closed') {
                    // The user explicitly cancelled the login action by closing an opened popup.
                    return this.error('The user closed the window.');
                } else if (!this.popUpDisabled) {
                    // tslint:disable-next-line:no-console
                    console.log('Popup authentication error: ', popUpError);
                }

                // PopUps might be blocked by the user, fallback to redirect
                try {
                    await this.userManager?.signinRedirect(this.createArguments(state));
                    return this.redirect();
                } catch (redirectError) {
                    // tslint:disable-next-line:no-console
                    console.log('Redirect authentication error: ', redirectError);
                    return this.error(redirectError);
                }
            }
        }
    }

    async completeSignIn(url: string) {
        try {
            await this.ensureUserManagerInitialized();
            const user = await this.userManager?.signinCallback(url);
            this.updateState(user);
            return this.success(user?.state);
        } catch (error) {
            // tslint:disable-next-line:no-console
            console.log('There was an error signing in: ', error);
            return this.error('There was an error signing in.');
        }
    }

    // We try to sign out the user in two different ways:
    // 1) We try to do a sign-out using a PopUp Window. This might fail if there is a
    //    Pop-Up blocker or the user has disabled PopUps.
    // 2) If the method above fails, we redirect the browser to the IdP to perform a traditional
    //    post logout redirect flow.
    async signOut(state: { returnUrl: string }): Promise<{status: string, message?: string}> {
        await this.ensureUserManagerInitialized();
        try {
            if (this.popUpDisabled) {
                throw new Error('Popup disabled. Change \'AuthorizeService.js:AuthorizeService.popupDisabled\' to false to enable it.');
            }

            await this.userManager?.signoutPopup(this.createArguments(undefined));
            this.updateState(undefined);
            return this.success(state);
        } catch (popupSignOutError) {
            // tslint:disable-next-line:no-console
            console.log('Popup signout error: ', popupSignOutError);
            try {
                await this.userManager?.signoutRedirect(this.createArguments(state));
                return this.redirect();
            } catch (redirectSignOutError) {
                // tslint:disable-next-line:no-console
                console.log('Redirect signout error: ', redirectSignOutError);
                return this.error(redirectSignOutError);
            }
        }
    }

    async completeSignOut(url: string) {
        await this.ensureUserManagerInitialized();
        try {
            const response = await this.userManager?.signoutCallback(url);
            this.updateState(undefined);
            return this.success((response as SignoutResponse).state);
        } catch (error) {
            // tslint:disable-next-line:no-console
            console.log(`There was an error trying to log out '${error}'.`);
            return this.error(error);
        }
    }

    updateState(user: User | undefined) {
        this.user = user;
        this.userIsAuthenticated = !!this.user;
        this.notifySubscribers();
    }

    subscribe(callback: () => void) {
        this.callbacks.push({ callback, subscription: this.nextSubscriptionId++ });
        return this.nextSubscriptionId - 1;
    }

    unsubscribe(subscriptionId: number): void {
        const subscriptionIndex = this.callbacks
            .map((element, index) => 
                element?.subscription === subscriptionId ? { found: true, index } : { found: false, index: -1 })
            .filter(element => element.found === true);
        if (subscriptionIndex.length !== 1) {
            throw new Error(`Found an invalid number of subscriptions ${subscriptionIndex.length}`);
        }

        this.callbacks = this.callbacks.splice(subscriptionIndex[0].index, 1);
    }

    notifySubscribers() {
        // tslint:disable-next-line:prefer-for-of
        for (let i = 0; i < this.callbacks.length; i++) {
            const callback = this.callbacks[i].callback;
            callback();
        }
    }

    createArguments(state: any) {
        return { useReplaceToNavigate: true, data: state };
    }

    error(message: string) {
        return { status: AuthenticationResultStatus.Fail, message };
    }

    success(state: { returnUrl: string }) {
        return { status: AuthenticationResultStatus.Success, state };
    }

    redirect() {
        return { status: AuthenticationResultStatus.Redirect };
    }

    async ensureUserManagerInitialized() {
        if (this.userManager !== undefined) {
            return;
        }

        const response = await fetch(ApplicationPaths.ApiAuthorizationClientConfigurationUrl);
        if (!response.ok) {
            throw new Error(`Could not load settings for '${ApplicationName}'`);
        }

        const settings = await response.json();
        settings.automaticSilentRenew = true;
        settings.includeIdTokenInSilentRenew = true;
        settings.userStore = new WebStorageStateStore({
            prefix: ApplicationName
        });

        this.userManager = new UserManager(settings);

        this.userManager.events.addUserSignedOut(async () => {
            await this.userManager?.removeUser();
            this.updateState(undefined);
        });

        this.userManager.events.addAccessTokenExpired(async () => {
            // tslint:disable-next-line:no-console
            console.log('token expired...');
            const result = await this.signIn({ returnUrl: this.getReturnUrl(undefined) });
            switch (result.status) {
                case AuthenticationResultStatus.Redirect:
                    // We replace the location here so that in case the user hits the back
                    // arrow from within the login page he doesn't get into an infinite
                    // redirect loop.
                    // window.location.replace(this.getReturnUrl());
                    break;
                case AuthenticationResultStatus.Success:
                    await this.navigateToReturnUrl('/');
                    break;
                case AuthenticationResultStatus.Fail:
                    break;
                default:
                    throw new Error(`Invalid status result ${result.status}.`);
            }
        });

        this.userManager.events.addAccessTokenExpiring( () => {
           // tslint:disable-next-line:no-console
            console.log('token expiring...');
        });
    }

    navigateToReturnUrl(returnUrl: string) {
        // It's important that we do a replace here so that we remove the callback uri with the
        // fragment containing the tokens from the browser history.
        window.location.replace(returnUrl);
    }

    getReturnUrl(state: any) {
        const params = new URLSearchParams(window.location.search);
        const fromQuery = params.get(QueryParameterNames.ReturnUrl);
        if (fromQuery && !fromQuery.startsWith(`${window.location.origin}/`)) {
            // This is an extra check to prevent open redirects.
            throw new Error('Invalid return url. The return url needs to have the same origin as the current page.');
        }
        return (state && state.returnUrl) || fromQuery || `${window.location.origin}/`;
    }
    static get instance() { return authService; }
}

const authService = new AuthorizeService();

export default authService;

export const AuthenticationResultStatus = {
    Fail: 'fail',
    Redirect: 'redirect',
    Success: 'success'
};
