import React, { Component } from 'react';
import TableData from './TableData';
import { PivotItem, Pivot } from 'office-ui-fabric-react';

class Home extends Component {          
    public render() {
        return (
            <Pivot>
                <PivotItem headerText="Samples">
                    <div>
                        <TableData isAuthenticated={false} title={"samples"} />
                    </div>
                </PivotItem>
                <PivotItem headerText="SDKs">
                    <div>
                        <TableData isAuthenticated={false} title={"sdks"} />
                    </div>
                </PivotItem>
            </Pivot>
        );
    }
}

export default Home;