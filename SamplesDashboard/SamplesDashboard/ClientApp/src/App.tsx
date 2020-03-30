import * as React from 'react';
import AuthorizeRoute  from './components/api-authorization/AuthorizeRoute';

import Layout from './components/layout/Layout';
import './custom.css';
import Samples from './views/Details';
import Home from './views/Home';

export default () => (
    <Layout>
        <AuthorizeRoute exact path='/' component={Home} />
        <AuthorizeRoute path='/samples/:name' component={Samples} />
    </Layout>
);
