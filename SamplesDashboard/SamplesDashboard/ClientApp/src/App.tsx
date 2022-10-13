// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { createTheme, initializeIcons, ThemeProvider } from '@fluentui/react';
import { Container } from 'react-bootstrap';
import { BrowserRouter as Router, Route } from 'react-router-dom';
import { MsalProvider } from '@azure/msal-react';
import { IPublicClientApplication } from '@azure/msal-browser';

import ProvideAppContext, { useAppContext } from './AppContext';
import ErrorMessage from './components/ErrorMessage/ErrorMessage';
import NavBar from './components/NavBar/NavBar';
import Home from './components/Home/Home';
import RepoDetails from './components/RepoDetails/RepoDetails';
import 'bootstrap/dist/css/bootstrap.css';

initializeIcons();

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
    white: '#212529',
  },
});

type AppProps = {
  pca?: IPublicClientApplication;
};

interface ThemedAppProps {
  children: React.ReactNode;
}

function ThemedApp({ children }: ThemedAppProps) {
  const context = useAppContext();

  const theme = context.theme === 'dark' ? darkTheme : undefined;

  return (
    <ThemeProvider theme={theme} applyTo='body'>
      {children}
    </ThemeProvider>
  );
}

function App({ pca }: AppProps) {
  return (
    <MsalProvider instance={pca!}>
      <ProvideAppContext>
        <ThemedApp>
          <Router>
            <NavBar />
            <Container fluid>
              <ErrorMessage />
              <Route exact path='/' render={(props) => <Home {...props} />} />
              <Route
                path='/repo/:name'
                render={(props) => <RepoDetails {...props} />}
              />
            </Container>
          </Router>
        </ThemedApp>
      </ProvideAppContext>
    </MsalProvider>
  );
}

export default App;
