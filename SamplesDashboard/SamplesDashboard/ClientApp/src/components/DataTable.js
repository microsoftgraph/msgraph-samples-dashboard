import React, { Component } from 'react';

export default class DataTable extends Component {
    constructor(props) {
        super(props);
        this.state = {
            tableData: []
        }
    }

    componentDidMount = () => {
        this.fetchData();
    }

    //fetching the data from the api
    fetchData = async () => {
        const response = await fetch('api/Samples');
        const data = await response.json();
        this.setState(
            {
                tableData: data
            });
    }
    //displaying sample data in the appropriate columns
    renderData() {
        return this.state.tableData.map(sample =>
            (
                <tr>
                    <td>{sample.name}</td>
                    <td>{sample.owner}</td>
                    <td>{sample.status}</td>
                    <td>{sample.language}</td>
                    <td>{sample.pullRequests}</td>
                    <td>{sample.issues}</td>
                    <td>{sample.stars}</td>q
                    <td>{sample.featureArea}</td>
                    <td>{sample.securityAlerts}</td>
                </tr>
            ),
        )
    }

    render() {
        return (
            <div>
                <table className="Table">
                    <thead>
                        <tr>
                            <th>Sample Name</th>
                            <th>Owner</th>
                            <th>Status</th>
                            <th>Language</th>
                            <th>Open Pull Requests</th>
                            <th>Open Issues</th>
                            <th>Stars</th>
                            <th>Feature Area</th>
                            <th>Security Alerts</th>
                        </tr>
                    </thead>
                    <tbody>
                        {this.renderData()}                        
                    </tbody>          
                </table>
            </div>
        );
    }
}