import React, { Component } from 'react';
import Repositories from './Repositories';
import { PivotItem, Pivot } from 'office-ui-fabric-react';

class Home extends Component {          
    public render() {
        return (
            <Pivot>
                <PivotItem headerText="Samples">
                    <div>
                        <Repositories isAuthenticated={false} title={"samples"} />
                    </div>
                </PivotItem>
                <PivotItem headerText="SDKs">
                    <div>
                        <Repositories isAuthenticated={false} title={"sdks"} />
                    </div>
                </PivotItem>
            </Pivot>
        );
    }
}

export default Home;