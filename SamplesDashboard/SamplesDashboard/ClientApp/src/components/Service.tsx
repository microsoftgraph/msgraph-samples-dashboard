import * as React from 'react';

interface IState {
    services: any;
}
export default class Service extends React.Component<{ sampleName: string }, IState> {
    constructor(props: { sampleName: string }) {
        super(props);
        this.state = {
            services: [],
        };
    }

    componentDidMount = () => {
        this.fetchData();
    }

    //fetching the data from the api
    fetchData = async () => {
        const response = await fetch('/feature/' + this.props.sampleName);
        const data = await response.json();
        this.setState(
            {
                services: data
            });
    }

    public render() {
        const services = this.state;
        return (<div>{services}</div>);

    }
}