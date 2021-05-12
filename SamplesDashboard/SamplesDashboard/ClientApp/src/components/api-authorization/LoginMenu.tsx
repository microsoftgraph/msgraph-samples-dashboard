import React, { Component, Fragment } from 'react';
import { NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import authService from './AuthorizeService';
import { ApplicationPaths } from './ApiAuthorizationConstants';

interface IState{
    isAuthenticated: boolean;
    userName: string | null | undefined;
}

export class LoginMenu extends Component<{}, IState> {
    private _subscription: number;
    
    constructor(props: {} ) {
        super(props);

        this._subscription = 0;
        this.state = {
            isAuthenticated: false,
            userName: null
        };
    }

    componentDidMount() {
        this._subscription = authService.subscribe(() => this.populateState());
        this.populateState();
    }

    componentWillUnmount() {
        authService.unsubscribe(this._subscription);
    }

    async populateState() {
        const [isAuthenticated, user] = await Promise.all([authService.isAuthenticated(), authService.getUser()]);
        this.setState({
            isAuthenticated,
            userName: user?.name
        });
    }

    render() {
        const { isAuthenticated, userName } = this.state;
        if (!isAuthenticated) {
            const registerPath = `${ApplicationPaths.Register}`;
            const loginPath = `${ApplicationPaths.Login}`;
            return this.anonymousView(registerPath, loginPath);
        } else {
            const profilePath = `${ApplicationPaths.Profile}`;
            const logoutPath = { pathname: `${ApplicationPaths.LogOut}`, state: { local: true } };
            
            if (userName) {
                return this.authenticatedView(userName, profilePath, logoutPath);
            }
            else {
                return this.anonymousView(`${ApplicationPaths.Register}`, `${ApplicationPaths.Login}`);
            }
        }
    }

    authenticatedView(userName: string, profilePath: string, logoutPath: { pathname: string, state: {local: boolean} }) 
    {
        return (<Fragment>
            <NavItem>
                <NavLink active tag={Link} to={profilePath}>Hello {userName}</NavLink>
            </NavItem>
            <NavItem>
                <NavLink active tag={Link} to={logoutPath}>Logout</NavLink>
            </NavItem>
        </Fragment>);

    }

    anonymousView(registerPath: string, loginPath: string) {
        return (<Fragment>            
            <NavItem>
                <NavLink active tag={Link} to={loginPath}>Login</NavLink>
            </NavItem>
        </Fragment>);
    }
}
