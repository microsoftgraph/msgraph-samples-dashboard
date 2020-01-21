import React, { Component } from 'react';
import DataTable from './DataTable';

class Home extends Component {          
    render() {
        return (
            <div>
                <h1> Samples Dashboard </h1>
                <br/>
                <h2> List of Samples </h2>
                <br />
                <DataTable  />
            </div>
        );
    }
}

export default Home;