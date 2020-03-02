import { DetailsListLayoutMode, Fabric, IColumn, 
    SelectionMode, ShimmeredDetailsList, PrimaryButton, FontIcon, mergeStyleSets, mergeStyles } from 'office-ui-fabric-react';
import * as React from 'react';

import PageTitle from '../components/layout/PageTitle';
import { IDetailsItem } from '../types/samples';

const iconClass = mergeStyles({
    fontSize: 15,
    height: 15,
    width: 15,
    margin: '0 5px'
});
const classNames = mergeStyleSets({
    yellow: [{ color: '#ffaa44' }, iconClass],
    green: [{ color: '#498205' }, iconClass],
    red: [{ color: '#d13438' }, iconClass],
    blue: [{ color: '#0078d4' }, iconClass]
});
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
        if (data.dependencyGraphManifests.nodes[0]) {
            this.allItems = data.dependencyGraphManifests.nodes[0].dependencies.nodes;
        }
        this.setState({
            items: this.allItems,
            repositoryDetails: {
                name: repositoryName,
                url: data.url
            },
            isLoading: false
        });
    }    
    

    public render(): JSX.Element {
        const { columns, items, repositoryDetails, isLoading } = this.state;
        
        return (
            <Fabric>
                <div>
                    <PageTitle title={repositoryDetails.name} />
                    { isLoading ?
                        <div /> :
                        <PrimaryButton href={repositoryDetails.url} target="_blank" > <FontIcon iconName="OpenInNewTab" className={iconClass} /> Go to Repository </PrimaryButton> 
                    }
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
}

function renderItemColumn(item: IDetailsItem, index: number | undefined, column: IColumn | undefined) {
    const col = column as IColumn;
    const packageName = item.packageName;
    const version = item.requirements;
    let currentVersion = "Unknown";
    if (item.repository && item.repository.releases && item.repository.releases.nodes[0]) {
        currentVersion = item.repository.releases.nodes[0].tagName;
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
            return checkStatus(status);
    }
}

function checkStatus(status: number)
{
    switch (status) {
        case 0:
            return <span><FontIcon iconName="StatusCircleQuestionMark" className={classNames.blue} /> Unknown </span>;

        case 1:
            return <span><FontIcon iconName="CompletedSolid" className={classNames.green} /> Up To Date </span>;

        case 2:
            return <span><FontIcon iconName="WarningSolid" className={classNames.yellow} /> Update </span>;

        case 3:
            return <span><FontIcon iconName="StatusErrorFull" className={classNames.red} /> Urgent Update </span>;
    }
}


