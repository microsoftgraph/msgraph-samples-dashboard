import React, { Component } from 'react';

export default class Language extends Component {
    constructor(props) {
        super(props);
        this.state = {
            tableData: [],
            sampleName: props.sampleName
        }
    }

    componentDidMount = () => {
        this.fetchData();
    }

    //fetching the data from the api
    fetchData = async () => {
        const response = await fetch('/language/'+this.state.sampleName);
        const data = await response.json();
        this.setState(
            {
                tableData: data
            });
    }

    render() {
        return (this.state.tableData);
}
}