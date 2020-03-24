import { DetailsListLayoutMode, Fabric, IColumn, 
    SelectionMode, ShimmeredDetailsList, PrimaryButton, FontIcon, mergeStyleSets, mergeStyles, TooltipHost } from 'office-ui-fabric-react';
import * as React from 'react';

import PageTitle from '../components/layout/PageTitle';
import { IDetailsItem } from '../types/samples';
import { Tooltip } from 'reactstrap';

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
        const { match: { params } } = this.props;
        const repositoryName = params.name;
        const columns: IColumn[] = [
            { key: 'packageName', name: 'Library', fieldName: 'packageName', minWidth: 300, maxWidth: 400, isRowHeader: true, 
                isResizable: true, isSorted: false, isSortedDescending: false, onColumnClick: this.onColumnClick
            },
            { key: 'requirements', name: 'Used version', fieldName: 'requirements', minWidth: 200, maxWidth: 300, 
                isResizable: true
            },
            { key: 'currentVersion', name: 'Latest version', fieldName: 'tagName', minWidth: 200, 
                maxWidth: 300, isResizable: true
            },
            { key: 'status', name: 'Status', fieldName: 'status', minWidth: 200, maxWidth: 300, 
                isResizable: true, onColumnClick: this.onColumnClick
            }
        ];
        // Adding Azure SDK column to SDK repository details 
        if ((repositoryName.includes('sdk')) || (repositoryName.includes('SDK'))) {
            const azureSdkColumn: IColumn = {
                key: 'azureSdkVersion', name: 'Azure SDK Version', fieldName: 'azureSdkVersion',
                minWidth: 200, maxWidth: 200, isResizable: true, onColumnClick: this.onColumnClick
            }
            // Adding the column to the second last position
            columns.splice(columns.length - 1, 0, azureSdkColumn);
        }

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
        // fetch repository libraries
    public fetchData = async () => {
        this.setState({ isLoading: true });
        const { match: { params } } = this.props;
        const repositoryName = params.name;
        const response = await fetch('api/repositories/' + repositoryName);
        const data = await response.json();
        if (data.dependencyGraphManifests.nodes[0]) {
            for (let index = 0; index < data.dependencyGraphManifests.nodes.length; index++) {
                data.dependencyGraphManifests.nodes[index].dependencies.nodes.forEach((element: any) => this.allItems.push(element));
            }
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
                    
                    { isLoading ?
                        <div /> :
                        <div>
                            <PageTitle title={`List of libraries in ${repositoryDetails.name}`} />
                            <PrimaryButton href={repositoryDetails.url} target="_blank" rel="noopener noreferrer"> <FontIcon iconName="OpenInNewTab" className={iconClass} /> Go to Repository </PrimaryButton> 
                        </div>
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

    private onColumnClick = (ev: React.MouseEvent<HTMLElement>, column: IColumn): void => {
        const { columns, items } = this.state;
        const newColumns: IColumn[] = columns.slice();
        const currColumn: IColumn = newColumns.filter(currCol => column.key === currCol.key)[0];
        newColumns.forEach((newCol: IColumn) => {
            if (newCol === currColumn) {
                currColumn.isSortedDescending = !currColumn.isSortedDescending;
                currColumn.isSorted = true;
            } else {
                newCol.isSorted = false;
                newCol.isSortedDescending = true;
            }
        });
        const newItems = copyAndSort(items, currColumn.fieldName!, currColumn.isSortedDescending);
        this.setState({
            columns: newColumns,
            items: newItems
        });
    };
}

function renderItemColumn(item: IDetailsItem, index: number | undefined, column: IColumn | undefined) {
    const col = column as IColumn;
    const packageName = item.packageName;
    const version = item.requirements;
    let currentVersion = item.latestVersion;
    const azureSdkVersion = item.azureSdkVersion;
    const status = item.status;
    const requirements = version.slice(2);

    switch (col.name) {
       
        case 'Library':
            return <span>{packageName} </span>;

        case 'Used version':
            return <span>{requirements} </span>;

        case 'Latest version':
            return <span>{currentVersion} </span>;

        case 'Azure SDK version':
            return <span>{azureSdkVersion}</span>;

        case 'Status':
            return checkStatus(status);
    }
}

function checkStatus(status: number)
{
    switch (status) {
        case 0:
            return <TooltipHost content="Unknown" id={'Unknown'}>
                <span><FontIcon iconName="StatusCircleQuestionMark" className={classNames.blue} /> Unknown </span>
            </TooltipHost>;

        case 1:
            return <TooltipHost content="This library is up to date" id={'UptoDate'}>
                <span><FontIcon iconName="CompletedSolid" className={classNames.green} /> Up To Date </span>
            </TooltipHost>;

        case 2:
            return <TooltipHost content="This library has a major/minor realease update" id={'Update'}>
                <span><FontIcon iconName="WarningSolid" className={classNames.yellow} /> Update </span>
            </TooltipHost>;

        case 3:
            return <TooltipHost content="This library has a patch release update" id={'UrgentUpdate'}>
                <span><FontIcon iconName="StatusErrorFull" className={classNames.red} /> Urgent Update </span>
            </TooltipHost>;
    }

}

function copyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
    const key = columnKey as keyof T;
    let itemsSorted = items.slice(0).sort((a: T, b: T) => (compare(a[key], b[key], isSortedDescending)));
    return itemsSorted;
}

function compare(a: any, b: any, isSortedDescending?: boolean) {
    // Handle the possible scenario of blank inputs 
    // and keep them at the bottom of the lists
    if (!a) return 1;
    if (!b) return -1;

    let valueA: any;
    let valueB: any;
    let comparison = 0;

    if (typeof a === 'string' || a instanceof String) {
        // Use toUpperCase() to ignore character casing
        valueA = a.toUpperCase();
        valueB = b.toUpperCase();
        // its an item of type number
    } else if (typeof a == 'number' && typeof b == 'number') {
        valueA = a;
        valueB = b;
    } else {
        // its an object which has a totalCount property
        valueA = a.totalCount;
        valueB = b.totalCount;
    }

    if (valueA > valueB) {
        comparison = 1;
    } else if (valueA < valueB) {
        comparison = -1;
    }

    if (isSortedDescending) {
        comparison = comparison * -1;
    }

    return comparison;
}



