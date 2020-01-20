import React, { Component } from 'react';
import DataTable from './DataTable';

class Home extends Component {
    render() {

        const headings = [
            'Sample name',
            'Owner',
            'Status',
            'Language',
            'Open Pull Requests',
            'Open Issues',
            'Stars',
            'Feature Area',
            'Security Alerts',
        ];

        const rows = [
            [
                'Sample 1',
                '@Bettirose',
                'Urgent Update',
                'C#',
                4,
                10,
                21,
                'InTune',
                'None',
            ],
            [
                'Sample 2',
                '@MaggieKim1',
                'Update',
                'JavaScript',
                1,
                3,
                15,
                'Outlook',
                2,
            ],
            [
                'Sample 3',
                '@ElinorW',
                'Up-to Date',
                'PHP',
                9,
                2,
                12,
                'Teams',
                4,
            ],
        ];

        return (
            <div>
                <h1> Samples Dashboard </h1>
                <br/>
                <h2> List of Samples </h2>
                <br/>
                <DataTable headings={headings} rows={rows} />
            </div>
        );
    }
}

export default Home;