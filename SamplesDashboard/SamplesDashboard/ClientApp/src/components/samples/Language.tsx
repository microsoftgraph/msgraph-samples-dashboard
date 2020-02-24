﻿import * as React from 'react';

interface IState {
    language: any;
}
export default class Language extends React.Component<{ sampleName: string }, IState> {
    constructor(props:{ sampleName: string }) {
        super(props);
        this.state = {
            language: [],    
        };
    }

    componentDidMount = () => {
        this.fetchData();
    }

    //fetching the data from the api
    fetchData = async () => {
        const response = await fetch('/features/' + this.props.sampleName);
        const data = await response.json();
        console.log(data);
        this.setState(
            {
                language: data
            });
    }

    public render() {
        const languages = this.state.language.languages.map((lang: string) => { return lang; }).join(", ");
        return (languages);              
    }
}
