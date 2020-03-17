import React, { Component } from 'react';
import Samples from './Samples';
import { PivotItem, Pivot } from 'office-ui-fabric-react';

class Home extends Component {          
    public render() {
        return (
            <Pivot>
                <PivotItem headerText="Samples">
                    <div>
                        <Samples isAuthenticated={false} title={"samples"} />
                    </div>
                </PivotItem>
                <PivotItem headerText="SDKs">
                    <div>
                        <Samples isAuthenticated={false} title={"sdks"} />
                    </div>
                </PivotItem>
            </Pivot>
        );
    }
}

export default Home;