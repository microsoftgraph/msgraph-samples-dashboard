import * as React from 'react';
import { Route } from 'react-router';
import Layout from './components/layout/Layout';
import Home from './views/Home';

import './custom.css'
import Samples from './views/Details';

export default () => (
    <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/samples/:name' component={Samples} />
    </Layout>
);
