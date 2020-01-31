import React, { Component } from 'react';
import ListData from './DetailList';

class Home extends Component {          
    render() {
        return (
            <div>
                <h1> Samples Dashboard </h1>
                <br/>
                <h2> List of Samples </h2>
                <br />
                <ListData />
            </div>
        );
    }
}

export default Home;