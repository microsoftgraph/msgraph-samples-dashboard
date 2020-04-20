import * as React from 'react';
import AuthorizeRoute  from './components/api-authorization/AuthorizeRoute';

import Layout from './components/layout/Layout';
import './custom.css';
import Samples from './views/Details';
import Home from './views/Home';
import { Route } from 'react-router';
import { ApplicationPaths } from './components/api-authorization/ApiAuthorizationConstants';
import ApiAuthorizationRoutes from './components/api-authorization/ApiAuthorizationRoutes';

export default () => (
    <Layout>
        <AuthorizeRoute exact path='/' component={Home} />
        <AuthorizeRoute path='/samples/:name' component={Samples} />
        <Route path={ApplicationPaths.ApiAuthorizationPrefix} component={ApiAuthorizationRoutes} />
    </Layout>
);