import React, { Component } from 'react';
import { Route, Redirect } from 'react-router-dom';
import { ApplicationPaths, QueryParameterNames } from './ApiAuthorizationConstants';
import authService from './AuthorizeService';

interface IState{
    authenticated: boolean;
    ready: boolean;
}

export default class AuthorizeRoute extends Component<any, IState> {
    private subscription: number;
    
    constructor(props: any) {
        super(props);

        this.subscription = 0;
        this.state = {
            authenticated: false,
            ready: false
        };
    }

    componentDidMount() {
        this.subscription = authService.subscribe(() => this.authenticationChanged());
        this.populateAuthenticationState();
    }

    componentWillUnmount() {
        authService.unsubscribe(this.subscription);
    }

    render() {
        const { ready, authenticated } = this.state;
        const redirectUrl = 
            `${ApplicationPaths.Login}?${QueryParameterNames.ReturnUrl}=${encodeURI(window.location.href)}`;
        if (!ready) {
            return <div></div>;
        } else {
            // tslint:disable-next-line
            const { component: Component, ...rest } = this.props;
            return <Route {...rest}
                render={(props) => {
                    if (authenticated) {
                        return <Component {...props} />;
                    } else {
                        return <Redirect to={redirectUrl} />;
                    }
                }} />;
        }
    }

    async populateAuthenticationState() {
        const authenticated = await authService.isAuthenticated();
        this.setState({ ready: true, authenticated });
    }

    async authenticationChanged() {
        this.setState({ ready: false, authenticated: false });
        await this.populateAuthenticationState();
    }
}
