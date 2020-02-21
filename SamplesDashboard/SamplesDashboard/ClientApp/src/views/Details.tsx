import { DetailsListLayoutMode, Fabric, IColumn, 
    SelectionMode, ShimmeredDetailsList } from 'office-ui-fabric-react';
import * as React from 'react';

import PageTitle from '../components/layout/PageTitle';
import { IDetailsItem } from '../types/samples';

export default class Details extends React.Component<any, any> {
    private allItems: IDetailsItem[];

    constructor(props: {}) {
        super(props);

        this.allItems = [];
        const columns: IColumn[] = [
            { key: 'packageName', name: 'Library', fieldName: 'packageName', minWidth: 300, maxWidth: 400, isRowHeader: true, 
                isResizable: true, isSorted: true, isSortedDescending: false },
            { key: 'requirements', name: 'Sample Version', fieldName: 'requirements', minWidth: 200, maxWidth: 300, 
                isResizable: true },
            { key: 'currentVersion', name: 'Current Version', fieldName: 'tagName', minWidth: 200, 
                maxWidth: 300, isResizable: true },
            { key: 'status', name: 'Status', fieldName: 'status', minWidth: 200, maxWidth: 300, 
                isResizable: true },         
        ];

        this.state = {
            columns,
            items: this.allItems,
            repositoryDetails: {},
            isLoading: false
        };
    }

    public async componentDidMount() {    
        this.fetchData();
    }
        // do the fetch 
    public fetchData = async () => {
        this.setState({ isLoading: true });
        const { match: { params } } = this.props;
        const repositoryName = params.name;
        const response = await fetch('api/samples/' + repositoryName);
        const data = await response.json();
        this.allItems = data;
        this.setState({
            items: this.allItems,
            repositoryDetails: {
                name: repositoryName
            },
            isLoading: false
        });
    }    
    

    public render(): JSX.Element {
        const { columns, items, repositoryDetails,isLoading } = this.state;
        return (
            <Fabric>
                <div>
                    <PageTitle title={repositoryDetails.name} />
                    <ShimmeredDetailsList
                        items={items}
                        columns={columns}
                        selectionMode={SelectionMode.none}
                        layoutMode={DetailsListLayoutMode.justified}
                        onRenderItemColumn={renderItemColumn}
                        isHeaderVisible={true}
                        enableShimmer={isLoading}
                    />
                </div>
            </Fabric>
        );
    }
} function renderItemColumn(item: IDetailsItem, index: number | undefined, column: IColumn | undefined) {
    const col = column as IColumn;
    const packageName = item.packageName;
    const version = item.requirements;
    const currentVersionObject = item.repository.releases.nodes[0];
    let currentVersion = "Unknown";
    if (currentVersionObject) {
        currentVersion = currentVersionObject.tagName;
    }

    const status = item.status;
    const requirements = version.slice(2);
    switch (col.name) {
       
        case 'Library':
            return <span>{packageName} </span>;

        case 'Sample Version':
            return <span>{requirements} </span>;

        case 'Current Version':
            return <span>{currentVersion} </span>;

        case 'Status':
            return <span>{status} </span>;

    }
}


