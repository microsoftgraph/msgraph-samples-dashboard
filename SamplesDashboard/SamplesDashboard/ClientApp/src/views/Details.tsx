import { Fabric } from 'office-ui-fabric-react';
import * as React from 'react';

import PageTitle from '../components/layout/PageTitle';

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
        });
      }

    public render(): JSX.Element {
        const { repositoryDetails } = this.state;

        return (
            <Fabric>
                <div>
                    <PageTitle title={repositoryDetails.name} />    
                </div>
            </Fabric>
        );
    }


}
