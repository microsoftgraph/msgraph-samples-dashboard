import React from 'react';
import Repositories from './repositories/Repositories';
import { PivotItem, Pivot, PivotLinkSize, FontSizes } from 'office-ui-fabric-react';
import queryString from 'query-string';


export default class Home extends React.Component<any> {
    public render(): JSX.Element {
        const { location } = this.props;
        let index = 0;
        const value = queryString.parse(location.search);
        if (value.tabIndex) {
            index = parseInt(value.tabIndex.toString());
        }

        return (
            <Pivot linkSize={PivotLinkSize.large}
                styles={{
                    text: {
                        fontSize: FontSizes.xLarge
                    },
                    root: {
                        marginBottom: '20px',
                    }
                }}
                defaultSelectedIndex = {index}
            >
                <PivotItem headerText="Samples" >
                    <div>
                        <Repositories isAuthenticated={true} title={"samples"} />
                    </div>
                </PivotItem>
                <PivotItem headerText="SDKs">
                    <div>
                        <Repositories isAuthenticated={true} title={"sdks"} />
                    </div>
                </PivotItem>
            </Pivot>
        );
    }
}
