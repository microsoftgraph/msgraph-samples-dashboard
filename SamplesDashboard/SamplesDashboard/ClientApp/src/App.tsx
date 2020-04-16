import * as React from 'react';
import { Route } from 'react-router';

import Layout from './components/layout/Layout';
import './custom.css';
import Details from './views/Details';
import Home from './views/Home';

export default () => (
    <Layout>
        <Route exact path='/' component={Home}/>
        <Route path='/samples/:name' component={Details} />
    </Layout>
);