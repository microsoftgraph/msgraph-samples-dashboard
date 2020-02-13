import * as React from 'react';
import { Fabric } from 'office-ui-fabric-react/lib/Fabric';

export default class SampleDependencies extends React.Component<any, any> {

    constructor(props: {}) {
        super(props);

        this.state = {
            repositoryDetails: {},
        };
    }

    public componentDidMount() {
        const { match: { params } } = this.props;
        const repositoryName = params.name;

        // do the fetch 

        // set details into the state object

        this.setState({
            repositoryDetails: {
                name: repositoryName
            }
        })
      }

    public render(): JSX.Element {
        const { repositoryDetails } = this.state;

        return (
            <Fabric>
                <div>
                    <h1>Samples Dashboard</h1>
                    <br />
                    <h3>{repositoryDetails.name}</h3>
                </div>
            </Fabric>
        );
    }


}
