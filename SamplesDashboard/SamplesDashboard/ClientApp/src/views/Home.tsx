import React, { Component } from 'react';
import Samples from './Samples';

class Home extends Component {          
    public render() {
        return (
            <div>
                <Samples isAuthenticated={false} />
            </div>
        );
    }
}

export default Home;