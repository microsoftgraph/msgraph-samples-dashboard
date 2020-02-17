import {
    DetailsListLayoutMode, Fabric, IColumn,
    SelectionMode, ShimmeredDetailsList
} from 'office-ui-fabric-react';
import * as React from 'react';

import PageTitle from '../components/layout/PageTitle';
import { IDetailsItem } from '../types/samples';

export default class Details extends React.Component<any, any> {
    private allItems: IDetailsItem[];

    constructor(props: {}) {
        super(props);

        this.allItems = [];
        const columns: IColumn[] = [
            {
                key: 'packageName', name: 'Library', fieldName: 'packageName', minWidth: 200, maxWidth: 300, isRowHeader: true,
                isResizable: true, isSorted: true, isSortedDescending: false
            },
            {
                key: 'requirements', name: 'Sample Version', fieldName: 'requirements', minWidth: 200, maxWidth: 300,
                isResizable: true
            },
            {
                key: 'currentVersion', name: 'Current Version', fieldName: 'currentVersion', minWidth: 200,
                maxWidth: 300, isResizable: true
            },
            {
                key: 'status', name: 'Status', fieldName: 'status', minWidth: 200, maxWidth: 300,
                isResizable: true
            },
        ];

        this.state = {
            columns,
            items: this.allItems,
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
        const { columns, items, repositoryDetails } = this.state;

        return (
            <Fabric>
                <div>
                    <PageTitle title={repositoryDetails.name} />
                    <ShimmeredDetailsList
                        items={items}
                        columns={columns}
                        selectionMode={SelectionMode.none}
                        layoutMode={DetailsListLayoutMode.justified}
                        isHeaderVisible={true}
                    />
                </div>
            </Fabric>
        );
    }
}


