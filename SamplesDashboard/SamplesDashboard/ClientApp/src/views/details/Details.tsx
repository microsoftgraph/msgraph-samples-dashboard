import {
    DetailsListLayoutMode,
    Fabric,
    FontIcon,
    FontSizes,
    IColumn,
    IDetailsHeaderProps,
    IRenderFunction,
    PrimaryButton,
    ScrollablePane,
    ScrollbarVisibility,
    SelectionMode,
    ShimmeredDetailsList,
    Sticky,
    StickyPositionType,
    TextField,
    TooltipHost
} from '@fluentui/react';
import * as React from 'react';
import { Link } from 'react-router-dom';
import authService from '../../components/api-authorization/AuthorizeService';
import PageTitle from '../../components/layout/PageTitle';
import { IDetailsItem } from '../../types/samples';
import { copyAndSort } from '../../utilities/copy-and-sort';
import { filterListClass } from '../repositories/Repositories.Styles';
import { buttonClass, classNames, descriptionClass, iconClass, linkClass } from './Details.Styles';
import { RepositoryStatus } from './Details.types';

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
            totalMinorUpdate: 0,
            totalMajorUpdate: 0,
            totalUrgentUpdate: 0,
            totalUnknown: 0,
            uptoDatePercent: 0,
            patchUpdatePercent: 0,
            minorUpdatePercent: 0,
            majorUpdatePercent: 0,
            urgentUpdatePercent: 0,
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
            items: copyAndSort(this.allItems, 'packageName'),
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
        let minorUpdateCount = 0;
        let majorUpdateCount = 0;
        let urgentUpdateCount = 0;
        let unknownCount = 0;

        if (items.length === 0) {
            return null;
        }
        for (const item of items) {
            switch (item.status) {
                case RepositoryStatus.unknown:
                    unknownCount++;
                    break;
                case RepositoryStatus.uptoDate:
                    uptoDateCount++;
                    break;
                case RepositoryStatus.minorUpdate:
                    minorUpdateCount++;
                    break;
                case RepositoryStatus.majorUpdate:
                    majorUpdateCount++;
                    break;
                case RepositoryStatus.patchUpdate:
                    patchUpdateCount++;
                    break;
                case RepositoryStatus.urgentUpdate:
                    urgentUpdateCount++;
                    break;
            }
        }
        const total = this.allItems.length;
        const uptoDateStats = parseFloat((uptoDateCount / total * 100).toFixed(1));
        const minorUpdateStats = parseFloat((minorUpdateCount / total * 100).toFixed(1));
        const majorUpdateStats = parseFloat((majorUpdateCount / total * 100).toFixed(1));
        const patchUpdateStats = parseFloat((patchUpdateCount / total * 100).toFixed(1));
        const urgentUpdateStats = parseFloat((urgentUpdateCount / total * 100).toFixed(1));
        const unknownStats = parseFloat((unknownCount / total * 100).toFixed(1));
        this.setState({
            totalUptoDate: uptoDateCount,
            totalMajorUpdate: majorUpdateCount,
            totalMinorUpdate: minorUpdateCount,
            totalPatchUpdate: patchUpdateCount,
            totalUnknown: unknownCount,
            totalUrgentUpdate: urgentUpdateCount,
            uptoDatePercent: uptoDateStats,
            majorUpdatePercent: majorUpdateStats,
            minorUpdatePercent: minorUpdateStats,
            patchUpdatePercent: patchUpdateStats,
            urgentUpdatePercent: urgentUpdateStats,
            unknownPercent: unknownStats
        });
    }
    public render(): JSX.Element {
        const { columns, items, repositoryDetails, isLoading, totalUptoDate, totalMajorUpdate, totalMinorUpdate, totalPatchUpdate,
            totalUnknown, totalUrgentUpdate, uptoDatePercent, minorUpdatePercent, majorUpdatePercent, patchUpdatePercent,
            urgentUpdatePercent, unknownPercent } = this.state;
        return (
            <div>
                    { isLoading ?
                    <div /> :
                    <Fabric>
                        {
                            repositoryDetails.name.includes('sdk') ?
                                <PrimaryButton className={buttonClass}>
                                    <Link to='/?tabIndex=2' className={linkClass}>
                                        <FontIcon iconName='Back' className={iconClass} /> Go Back </Link>
                                </PrimaryButton> :
                                <PrimaryButton className={buttonClass}>
                                    <Link to='/' className={linkClass}>
                                        <FontIcon iconName='Back' className={iconClass} />Go Back </Link>
                                </PrimaryButton>
                        }
                        <PrimaryButton className={buttonClass}>
                            <a href={repositoryDetails.url} target='_blank' rel='noopener noreferrer' className={linkClass}>
                                <FontIcon iconName='OpenInNewTab' className={iconClass} />Go to Repository</a>
                        </PrimaryButton>
                        <div className='row'>
                            <div className='col-sm-2'>
                                <div className='card'>
                                    <div className='card-body'>
                                        <p className='card-text'>
                                            <FontIcon iconName='StatusCircleInner' className={classNames.green} />
                                             Up to date: {totalUptoDate}
                                        </p>
                                        <div className='card-footer'>
                                            <span className={classNames.green}>{uptoDatePercent}%</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div className='col-sm-2'>
                                <div className='card'>
                                    <div className='card-body'>
                                        <p className='card-text'>
                                            <FontIcon iconName='StatusCircleInner' className={classNames.yellowGreen} />
                                             Patch Updates: {totalPatchUpdate}
                                        </p>
                                        <div className='card-footer'>
                                            <span className={classNames.yellowGreen}>{patchUpdatePercent}%</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div className='col-sm-2'>
                                <div className='card'>
                                    <div className='card-body'>

                                        <p className='card-text'>
                                            <FontIcon iconName='StatusCircleInner' className={classNames.yellow} />
                                            Minor Updates: {totalMinorUpdate}
                                        </p>
                                        <div className='card-footer'>
                                            <span className={classNames.yellow}>
                                                {minorUpdatePercent}%
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div className='col-sm-2'>
                                <div className='card'>
                                    <div className='card-body'>

                                        <p className='card-text'>
                                            <FontIcon iconName='StatusCircleInner' className={classNames.orange} />
                                            Major Updates: {totalMajorUpdate}
                                        </p>
                                        <div className='card-footer'>
                                            <span className={classNames.orange}>
                                                {majorUpdatePercent}%
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div className='col-sm-2'>
                                <div className='card'>
                                    <div className='card-body'>
                                        <p className='card-text'>
                                            <FontIcon iconName='StatusCircleInner' className={classNames.red} />
                                            Security alerts: {totalUrgentUpdate}
                                        </p>
                                        <div className='card-footer'>
                                            <span className={classNames.red}>{urgentUpdatePercent}% </span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div className='col-sm-2'>
                                <div className='card'>
                                    <div className='card-body'>
                                        <p className='card-text'>
                                            <FontIcon iconName='StatusCircleInner' className={classNames.blue} />
                                            Unknown versions: {totalUnknown}
                                        </p>
                                        <div className='card-footer'>
                                            <span className={classNames.blue}>{unknownPercent}% </span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <PageTitle title={`List of ${this.allItems.length} libraries in ${repositoryDetails.name}`} />
                        <div className={descriptionClass}> {repositoryDetails.description} </div>
                        <div className={classNames.wrapper}>
                            {this.allItems.length === 0 ?
                                <div style={{
                                    display: 'flex',
                                    width: '100%',
                                    minHeight: '200px',
                                    justifyContent: 'center',
                                    alignItems: 'center',
                                    fontSize: FontSizes.large
                                }}>
                                    <p>No data available</p>
                                </div> :
                                <ScrollablePane scrollbarVisibility={ScrollbarVisibility.auto}>
                                    <Sticky stickyPosition={StickyPositionType.Header}>
                                        <TextField
                                            className={filterListClass}
                                            label='Filter by library:'
                                            onChange={this.onFilterName}
                                            styles={{ root: { maxWidth: '300px' } }}
                                        />
                                    </Sticky>
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
                                }
                        </div>
                    </Fabric>
                    }
            </div>
        );
    }

    private onFilterName = (ev: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, text?:
        string | undefined): void => {
        this.setState({
            items: text ? this.allItems.filter(i => i.packageName.toLowerCase()
                .indexOf(text.toLowerCase()) > -1) : this.allItems
        });
    };

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
    }
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
            return <TooltipHost content='This library has a patch update' id={'PatchUpdate'}>
                <span><FontIcon iconName='StatusCircleInner' className={classNames.yellowGreen} /> Patch Update </span>
            </TooltipHost>;

        case 3:
            return <TooltipHost content='This library has a minor release update' id={'MinorUpdate'}>
                <span><FontIcon iconName='StatusCircleInner' className={classNames.yellow} /> Minor Update </span>
            </TooltipHost>;

        case 4:
            return <TooltipHost content='This library has a major release update.' id={'Update'}>
                <span><FontIcon iconName='StatusCircleInner' className={classNames.orange} /> Major Update </span>
            </TooltipHost>;

        case 5:
            return <TooltipHost content='This library has a security alert. Please go to GitHub to update.'
             id={'UrgentUpdate'}>
                <span><FontIcon iconName='StatusCircleInner' className={classNames.red} /> Urgent Update </span>
            </TooltipHost>;
    }
}




