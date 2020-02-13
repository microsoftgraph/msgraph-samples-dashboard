import React, { Component } from 'react';
import SampleList from './Samples';

class Home extends Component {          
    render() {
        return (
            <div>
                <h1> Samples Dashboard </h1>
                <br/>
                <h2> List of Samples </h2>
                <br />
                <SampleList />
            </div>
        );
    }
}

export default Home;