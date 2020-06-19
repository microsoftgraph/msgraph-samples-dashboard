import { UserManager, WebStorageStateStore } from 'oidc-client';
import { QueryParameterNames, ApplicationPaths, ApplicationName } from './ApiAuthorizationConstants';

export class AuthorizeService {
    callbacks = [];
    nextSubscriptionId = 0;
    user = null;
    userIsAuthenticated = false;

    // By default pop ups are disabled because they don't work properly on Edge.
    // If you want to enable pop up authentication simply set this flag to false.
    popUpDisabled = true;

    async isAuthenticated() {
        const user = await this.getUser();
        return !!user;
    }

    async getUser() {
        if (this.user && this.user.profile) {
            return this.user.profile;
        }

        await this.ensureUserManagerInitialized();
        const user = await this.userManager.getUser();
        return user && user.profile;
    }

    async getAccessToken() {
        await this.ensureUserManagerInitialized();
        const user = await this.userManager.getUser();
        return user && user.access_token;
    }

    // We try to authenticate the user in three different ways:
    // 1) We try to see if we can authenticate the user silently. This happens
    //    when the user is already logged in on the IdP and is done using a hidden iframe
    //    on the client.
    // 2) We try to authenticate the user using a PopUp Window. This might fail if there is a
    //    Pop-Up blocker or the user has disabled PopUps.
    // 3) If the two methods above fail, we redirect the browser to the IdP to perform a traditional
    //    redirect flow.
    async signIn(state) {
        await this.ensureUserManagerInitialized();
        try {
            const silentUser = await this.userManager.signinSilent(this.createArguments());
            this.updateState(silentUser);
            return this.success(state);
        } catch (silentError) {
            // User might not be authenticated, fallback to popup authentication
            // tslint:disable-next-line:no-console
            console.log("Silent authentication error: ", silentError);

            try {
                if (this.popUpDisabled) {
                    throw new Error('Popup disabled. Change \'AuthorizeService.js:AuthorizeService.popupDisabled\' to false to enable it.')
                }

                const popUpUser = await this.userManager.signinPopup(this.createArguments());
                this.updateState(popUpUser);
                return this.success(state);
            } catch (popUpError) {
                if (popUpError.message === "Popup window closed") {
                    // The user explicitly cancelled the login action by closing an opened popup.
                    return this.error("The user closed the window.");
                } else if (!this.popUpDisabled) {
                    // tslint:disable-next-line:no-console
                    console.log("Popup authentication error: ", popUpError);
                }

                // PopUps might be blocked by the user, fallback to redirect
                try {
                    await this.userManager.signinRedirect(this.createArguments(state));
                    return this.redirect();
                } catch (redirectError) {
                    // tslint:disable-next-line:no-console
                    console.log("Redirect authentication error: ", redirectError);
                    return this.error(redirectError);
                }
            }
        }
    }

    async completeSignIn(url) {
        try {
            await this.ensureUserManagerInitialized();
            const user = await this.userManager.signinCallback(url);
            this.updateState(user);
            return this.success(user && user.state);
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
    async signOut(state) {
        await this.ensureUserManagerInitialized();
        try {
            if (this.popUpDisabled) {
                throw new Error('Popup disabled. Change \'AuthorizeService.js:AuthorizeService.popupDisabled\' to false to enable it.')
            }

            await this.userManager.signoutPopup(this.createArguments());
            this.updateState(undefined);
            return this.success(state);
        } catch (popupSignOutError) {
            // tslint:disable-next-line:no-console
            console.log("Popup signout error: ", popupSignOutError);
            try {
                await this.userManager.signoutRedirect(this.createArguments(state));
                return this.redirect();
            } catch (redirectSignOutError) {
                // tslint:disable-next-line:no-console
                console.log("Redirect signout error: ", redirectSignOutError);
                return this.error(redirectSignOutError);
            }
        }
    }

    async completeSignOut(url) {
        await this.ensureUserManagerInitialized();
        try {
            const response = await this.userManager.signoutCallback(url);
            this.updateState(null);
            return this.success(response && response.data);
        } catch (error) {
            // tslint:disable-next-line:no-console
            console.log(`There was an error trying to log out '${error}'.`);
            return this.error(error);
        }
    }

    updateState(user) {
        this.user = user;
        this.userIsAuthenticated = !!this.user;
        this.notifySubscribers();
    }

    subscribe(callback) {
        this.callbacks.push({ callback, subscription: this.nextSubscriptionId++ });
        return this.nextSubscriptionId - 1;
    }

    unsubscribe(subscriptionId) {
        const subscriptionIndex = this.callbacks
            .map((element, index) => element.subscription === subscriptionId ? { found: true, index } : { found: false })
            .filter(element => element.found === true);
        if (subscriptionIndex.length !== 1) {
            throw new Error(`Found an invalid number of subscriptions ${subscriptionIndex.length}`);
        }

        this.callbacks = this.callbacks.splice(subscriptionIndex[0].index, 1);
    }

    notifySubscribers() {
        for (let i = 0; i < this.callbacks.length; i++) {
            const callback = this.callbacks[i].callback;
            callback();
        }
    }

    createArguments(state) {
        return { useReplaceToNavigate: true, data: state };
    }

    error(message) {
        return { status: AuthenticationResultStatus.Fail, message };
    }

    success(state) {
        return { status: AuthenticationResultStatus.Success, state };
    }

    redirect() {
        return { status: AuthenticationResultStatus.Redirect };
    }

    async ensureUserManagerInitialized() {
        if (this.userManager !== undefined) {
            return;
        }

        let response = await fetch(ApplicationPaths.ApiAuthorizationClientConfigurationUrl);
        if (!response.ok) {
            throw new Error(`Could not load settings for '${ApplicationName}'`);
        }

        let settings = await response.json();
        settings.automaticSilentRenew = true;
        settings.includeIdTokenInSilentRenew = true;
        settings.userStore = new WebStorageStateStore({
            prefix: ApplicationName
        });

        this.userManager = new UserManager(settings);

        this.userManager.events.addUserSignedOut(async () => {
            await this.userManager.removeUser();
            this.updateState(undefined);
        });

        this.userManager.events.addAccessTokenExpired(async () => {
            // tslint:disable-next-line:no-console
            console.log("token expired...");
            var result = await this.signIn({ returnUrl: this.getReturnUrl() });
            switch (result.status) {
                case AuthenticationResultStatus.Redirect:
                    // We replace the location here so that in case the user hits the back
                    // arrow from within the login page he doesn't get into an infinite
                    // redirect loop.
                    //window.location.replace(this.getReturnUrl());
                    break;
                case AuthenticationResultStatus.Success:
                    await this.navigateToReturnUrl("/");
                    break;
                case AuthenticationResultStatus.Fail:
                    break;
                default:
                    throw new Error(`Invalid status result ${result.status}.`);
            }
        });

        this.userManager.events.addAccessTokenExpiring(function () {
           // tslint:disable-next-line:no-console
            console.log("token expiring...");
        });
    }

    navigateToReturnUrl(returnUrl) {
        // It's important that we do a replace here so that we remove the callback uri with the
        // fragment containing the tokens from the browser history.
        window.location.replace(returnUrl);
    }

    getReturnUrl(state) {
        const params = new URLSearchParams(window.location.search);
        const fromQuery = params.get(QueryParameterNames.ReturnUrl);
        if (fromQuery && !fromQuery.startsWith(`${window.location.origin}/`)) {
            // This is an extra check to prevent open redirects.
            throw new Error("Invalid return url. The return url needs to have the same origin as the current page.");
        }
        return (state && state.returnUrl) || fromQuery || `${window.location.origin}/`;
    }
    static get instance() { return authService }
}

const authService = new AuthorizeService();

export default authService;

export const AuthenticationResultStatus = {
    Fail: 'fail',
    Redirect: 'redirect',
    Success: 'success'
};
