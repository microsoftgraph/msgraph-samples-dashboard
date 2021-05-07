//import { FontSizes, Pivot, PivotItem, PivotLinkSize } from 'office-ui-fabric-react';
import { FontSizes, Pivot, PivotItem } from '@fluentui/react';
import queryString from 'query-string';
import React from 'react';
import Repositories from './repositories/Repositories';


export default class Home extends React.Component<any> {
    public render(): JSX.Element {
        const { location } = this.props;
        let index = 0;
        const value = queryString.parse(location.search);
        if (value.tabIndex) {
            index = parseInt(value.tabIndex.toString(), 10);
        }

        return (
            <Pivot linkSize="large"
                styles={{
                    text: {
                        fontSize: FontSizes.xLarge
                    },
                    root: {
                        marginBottom: '20px',
                    }
                }}
                defaultSelectedKey = "1"
            >
                <PivotItem headerText='Samples' itemKey="1" >
                    <div>
                        <Repositories isAuthenticated={true} title={'samples'} />
                    </div>
                </PivotItem>
                <PivotItem headerText='SDKs' itemKey="2">
                    <div>
                        <Repositories isAuthenticated={true} title={'sdks'} />
                    </div>
                </PivotItem>
            </Pivot>
        );
    }
}
