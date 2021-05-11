import * as React from 'react';
import { createTheme, PartialTheme, ThemeProvider } from '@fluentui/react';
import AuthorizeRoute  from './components/api-authorization/AuthorizeRoute';

import { Route } from 'react-router';
import { ApplicationPaths } from './components/api-authorization/ApiAuthorizationConstants';
import ApiAuthorizationRoutes from './components/api-authorization/ApiAuthorizationRoutes';
import Layout from './components/layout/Layout';
import './custom.css';
import Details from './views/details/Details';
import Home from './views/Home';

const darkTheme = createTheme({
    palette: {
        themePrimary: '#5898c9',
        themeLighterAlt: '#040608',
        themeLighter: '#0e1820',
        themeLight: '#1b2e3c',
        themeTertiary: '#355c79',
        themeSecondary: '#4e86b1',
        themeDarkAlt: '#67a2cf',
        themeDark: '#7bafd6',
        themeDarker: '#9ac2e1',
        neutralLighterAlt: '#212121',
        neutralLighter: '#2a2a2a',
        neutralLight: '#393939',
        neutralQuaternaryAlt: '#424242',
        neutralQuaternary: '#494949',
        neutralTertiaryAlt: '#686868',
        neutralTertiary: '#ececec',
        neutralSecondary: '#efefef',
        neutralPrimaryAlt: '#f2f2f2',
        neutralPrimary: '#e3e3e3',
        neutralDark: '#f9f9f9',
        black: '#fcfcfc',
        white: '#171717',
    }
});

export default class App extends React.Component<any, { currentTheme: PartialTheme | undefined }> {

    constructor(props: {}) {
        super(props);

        const currentTheme = localStorage.getItem('selectedTheme');

        this.state = {
            currentTheme: currentTheme === 'dark' ? darkTheme : undefined,
        }
    }

    public setTheme(theme: string): void {
        localStorage.setItem('selectedTheme', theme);

        switch (theme) {
            case 'dark':
                this.setState({ currentTheme: darkTheme });
                break;
            case 'light':
            default:
                this.setState({ currentTheme: undefined });
        }
    }

    public render(): JSX.Element {
        return (
            <ThemeProvider theme={this.state?.currentTheme} applyTo='body' >
                <Layout setThemeMethod={(theme: string) => this.setTheme(theme)} >
                    <AuthorizeRoute exact path='/' component={Home} />
                    <AuthorizeRoute path='/samples/:name' component={Details} />
                    <Route path={ApplicationPaths.ApiAuthorizationPrefix} component={ApiAuthorizationRoutes} />
                </Layout>
            </ThemeProvider>
        );
    }
}