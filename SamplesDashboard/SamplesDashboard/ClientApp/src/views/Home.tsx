import React, { Component } from 'react';
import Samples from './Samples';
import { PivotItem, Pivot } from 'office-ui-fabric-react';
import SDKs from './SDKs';

class Home extends Component {          
    public render() {
        return (
            <Pivot>
                <PivotItem headerText="Samples">
                    <div>
                        <Samples isAuthenticated={false} />
                    </div>
                </PivotItem>
                <PivotItem headerText="SDKs">
                    <div>
                        <SDKs isAuthenticated={false} />
                    </div>
                </PivotItem>
            </Pivot>
        );
    }
}

export default Home;