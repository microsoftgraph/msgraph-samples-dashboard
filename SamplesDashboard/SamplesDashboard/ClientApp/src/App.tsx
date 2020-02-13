import * as React from 'react';
import { Route } from 'react-router';
import Layout from './components/Layout';
import Home from './components/Home';

import './custom.css'
import Samples from './components/Samples';

export default () => (
    <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/samples/:name' component={Samples} />
    </Layout>
);
