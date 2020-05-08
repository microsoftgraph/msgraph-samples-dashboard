import { DetailsListLayoutMode, Fabric, FontIcon, 
    IColumn, IDetailsHeaderProps, IRenderFunction, PrimaryButton,
    ScrollablePane, ScrollbarVisibility, SelectionMode, ShimmeredDetailsList, Sticky,
    StickyPositionType, TooltipHost } from 'office-ui-fabric-react';
import * as React from 'react';

import { Link } from 'react-router-dom';
import authService from '../../components/api-authorization/AuthorizeService';
import PageTitle from '../../components/layout/PageTitle';
import { IDetailsItem } from '../../types/samples';
import { buttonClass, classNames, descriptionClass, iconClass, linkClass } from './Details.Styles';

export default class Details extends React.Component<any, any> {
    private allItems: IDetailsItem[];

    constructor(props: {}) {
        super(props);

        this.allItems = [];
        const { match: { params } } = this.props;
        const repositoryName = params.name;
        const columns: IColumn[] = [
            { key: 'packageName', name: 'Library', fieldName: 'packageName', minWidth: 300, maxWidth: 400, 
            isRowHeader: true, isResizable: true, isSorted: false, isSortedDescending: false, 
            onColumnClick: this.onColumnClick
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
                key: 'azureSdkVersion', name: 'Azure SDK version', fieldName: 'azureSdkVersion',
                minWidth: 200, maxWidth: 200, isResizable: true, onColumnClick: this.onColumnClick
            };
            // Adding the column to the second last position
            columns.splice(columns.length - 1, 0, azureSdkColumn);
        }

        this.state = {
            columns,
            items: this.allItems,
            repositoryDetails: {},
            isLoading: true,
            totalUptoDate: 0,
            totalPatchUpdate: 0,
            totalMajorUpdate: 0,
            totalUnknown: 0,
            uptoDatePercent: 0,
            patchUpdatePercent: 0,
            majorUpdatePercent: 0,
            unknownPercent: 0
        };
    }

    public async componentDidMount() {    
        this.fetchData();
    }

    // fetch repository libraries
    public fetchData = async () => {
        const { match: { params } } = this.props;
        const repositoryName = params.name;

        const token = await authService.getAccessToken();
        const response = await fetch('api/repositories/' + repositoryName,
            {
                headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
            }
        );
        const data = await response.json();
        if (data.dependencyGraphManifests.nodes[0]) {
            let index;
            for (index in data.dependencyGraphManifests.nodes) {
                if (data.dependencyGraphManifests.nodes.hasOwnProperty(index)) {
                    data.dependencyGraphManifests.nodes[index].dependencies.nodes.forEach((element: any) =>
                        this.allItems.push(element)); 
                }               
            }
        }
        // call the statistics function
        this.statusStatistics(this.allItems);
        this.setState({
            items: this.allItems,
            repositoryDetails: {
                name: repositoryName,
                description: data.description,
                url: data.url
            },
            isLoading: false
        });
    }

    // compute status statistics 
    public statusStatistics(items: IDetailsItem[]) {
        let uptoDateCount = 0;
        let patchUpdateCount = 0;
        let majorUpdateCount = 0;
        let unknownCount = 0;
        for (const item of items) {
            switch (item.status) {
                    case 1:
                        uptoDateCount = uptoDateCount + 1;
                        break;
                    case 2:
                        majorUpdateCount = majorUpdateCount + 1;
                        break;
                    case 3:
                        patchUpdateCount = patchUpdateCount + 1;
                        break;
                    case 0:
                        unknownCount = unknownCount + 1;
                        break;
                }
            }
        const total = this.allItems.length;
        const uptoDateStats = parseFloat((uptoDateCount / total * 100).toFixed(1));
        const patchUpdateStats = parseFloat((patchUpdateCount / total * 100).toFixed(1));
        const majorUpdateStats = parseFloat((majorUpdateCount / total * 100).toFixed(1));
        const unknownStats = parseFloat((unknownCount / total * 100).toFixed(1));
        this.setState({
            totalUptoDate: uptoDateCount,
            totalPatchUpdate: patchUpdateCount,
            totalMajorUpdate: majorUpdateCount,
            totalUnknown: unknownCount,
            uptoDatePercent: uptoDateStats,
            patchUpdatePercent: patchUpdateStats,
            majorUpdatePercent: majorUpdateStats,
            unknownPercent: unknownStats
        });
    }
    public render(): JSX.Element {
        const { columns, items, repositoryDetails, isLoading, totalUptoDate, totalPatchUpdate, totalMajorUpdate, 
            totalUnknown, uptoDatePercent, patchUpdatePercent, majorUpdatePercent, unknownPercent  } = this.state;
        return (
            <div>     
                    { isLoading ?
                    <div /> :
                    <Fabric>
                        {
                            repositoryDetails.name.includes('sdk') ?
                                <PrimaryButton className={buttonClass}>
                                    <Link to='/?tabIndex=1' className={linkClass}>
                                        <FontIcon iconName='Back' className={iconClass} /> Go Back </Link>
                                </PrimaryButton> :
                                <PrimaryButton className={buttonClass}>
                                    <Link to='/' className={linkClass}>
                                        <FontIcon iconName='Back' className={iconClass} />Go Back </Link>
                                </PrimaryButton>
                        }
                        <PrimaryButton href={repositoryDetails.url} target='_blank' rel='noopener noreferrer'
                            className={buttonClass}>
                            <FontIcon iconName='OpenInNewTab' className={iconClass} />
                                Go to Repository
                        </PrimaryButton>
                        <div className='row'>
                            <div className='col-sm-3'>
                                <div className='card'>
                                    <div className='card-body'>
                                        <p className='card-text'>
                                            <FontIcon iconName='StatusCircleInner' className={classNames.green} />
                                            Libraries that are up to date: {totalUptoDate}
                                        </p>
                                        <div className='card-footer'>
                                            <span className={classNames.green}>{uptoDatePercent}% Up To Date</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div className='col-sm-3'>
                                <div className='card'>
                                    <div className='card-body'>
                                        <p className='card-text'>
                                            <FontIcon iconName='StatusCircleInner' className={classNames.red} />
                                            Libraries with a patch update: {totalPatchUpdate}
                                        </p>
                                        <div className='card-footer'>
                                            <span className={classNames.red}>{patchUpdatePercent}% Patch Update</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div className='col-sm-3'>
                                <div className='card'>
                                    <div className='card-body'>

                                        <p className='card-text'>
                                            <FontIcon iconName='StatusCircleInner' className={classNames.yellow} />
                                            Libraries with a mijor/minor update: {totalMajorUpdate}
                                        </p>
                                        <div className='card-footer'>
                                            <span className={classNames.yellow}>
                                                {majorUpdatePercent}% Major/Minor Update
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div className='col-sm-3'>
                                <div className='card'>
                                    <div className='card-body'>
                                        <p className='card-text'>
                                            <FontIcon iconName='StatusCircleInner' className={classNames.blue} />
                                            Libraries with unknown versions: {totalUnknown}
                                        </p>
                                        <div className='card-footer'>
                                            <span className={classNames.blue}>{unknownPercent}% Uknown</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <PageTitle title={`List of libraries in ${repositoryDetails.name}`} />
                        <div className={descriptionClass}> {repositoryDetails.description} </div>   
                        <p className={classNames.detailList}>There are {this.allItems.length} libraries listed </p>
                        <div className={classNames.wrapper}>
                            <ScrollablePane scrollbarVisibility={ScrollbarVisibility.auto}>
                                <div>
                                    <ShimmeredDetailsList
                                        items={items}
                                        columns={columns}
                                        selectionMode={SelectionMode.none}
                                        layoutMode={DetailsListLayoutMode.justified}
                                        isHeaderVisible={true}
                                        onRenderItemColumn={renderItemColumn}
                                        enableShimmer={isLoading}
                                        onRenderDetailsHeader={onRenderDetailsHeader}
                                    />
                                </div>
                            </ScrollablePane>   
                        </div>
                    </Fabric>
                    }
            </div>
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
// Enables the column headers to remain sticky
const onRenderDetailsHeader: IRenderFunction<IDetailsHeaderProps> = (props, defaultRender) => {
    if (!props) {
        return null;
    }
    return (
        <Sticky stickyPosition={StickyPositionType.Header} isScrollSynced={true}>
            {defaultRender!({
                ...props
            })}
        </Sticky>
    );
};

function renderItemColumn(item: IDetailsItem, index: number | undefined, column: IColumn | undefined) {
    const col = column as IColumn;
    const packageName = item.packageName;
    const version = item.requirements;
    const currentVersion = item.latestVersion;
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
// checks the value of the status and displays the appropriate status
function checkStatus(status: number)
{
    switch (status) {
        case 0:
            return <TooltipHost content='Unknown' id={'Unknown'}>
                <span><FontIcon iconName='StatusCircleInner' className={classNames.blue} /> Unknown </span>
            </TooltipHost>;

        case 1:
            return <TooltipHost content='This library is up to date' id={'UptoDate'}>
                <span><FontIcon iconName='StatusCircleInner' className={classNames.green} /> Up To Date </span>
            </TooltipHost>;

        case 2:
            return <TooltipHost content='This library has a major/minor release update' id={'Update'}>
                <span><FontIcon iconName='StatusCircleInner' className={classNames.yellow} /> Update </span>
            </TooltipHost>;

        case 3:
            return <TooltipHost content='This library has a patch release update' id={'UrgentUpdate'}>
                <span><FontIcon iconName='StatusCircleInner' className={classNames.red} /> Urgent Update </span>
            </TooltipHost>;
    }
}

function copyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
    const key = columnKey as keyof T;
    const itemsSorted = items.slice(0).sort((a: T, b: T) => (compare(a[key], b[key], isSortedDescending)));
    return itemsSorted;
}

function compare(a: any, b: any, isSortedDescending?: boolean) {
    // Handle the possible scenario of blank inputs 
    // and keep them at the bottom of the lists
    if (!a) { return 1; }
    if (!b) { return -1; }

    let valueA: any;
    let valueB: any;
    let comparison = 0;

    if (typeof a === 'string' || a instanceof String) {
        // Use toUpperCase() to ignore character casing
        valueA = a.toUpperCase();
        valueB = b.toUpperCase();
        // its an item of type number
    } else if (typeof a === 'number' && typeof b === 'number') {
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



