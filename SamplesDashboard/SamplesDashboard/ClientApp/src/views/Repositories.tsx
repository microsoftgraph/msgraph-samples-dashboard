import {
    DetailsListLayoutMode, IColumn,
    SelectionMode, ShimmeredDetailsList, FontIcon, IRenderFunction, IDetailsHeaderProps, TooltipHost
} from 'office-ui-fabric-react';
import { Fabric } from 'office-ui-fabric-react/lib/Fabric';
import { initializeIcons } from 'office-ui-fabric-react/lib/Icons';
import { ScrollablePane, ScrollbarVisibility } from 'office-ui-fabric-react/lib/ScrollablePane';
import { Sticky, StickyPositionType } from 'office-ui-fabric-react/lib/Sticky';
import { mergeStyles, mergeStyleSets } from 'office-ui-fabric-react/lib/Styling';
import { TextField } from 'office-ui-fabric-react/lib/TextField';
import * as React from 'react';
import { Link } from 'react-router-dom';

import PageTitle from '../components/layout/PageTitle';
import { IRepositoryItem, IRepositoryState } from '../types/samples';
import authService from '../components/api-authorization/AuthorizeService';

initializeIcons();

const filterListClass = mergeStyles({
    display: 'block',
    padding: '10px'
});

const iconClass = mergeStyles({
    fontSize: 15,
    height: 15,
    width: 15,
    margin: '0 5px'
});

const classNames = mergeStyleSets({
    wrapper: {
        background: '#fff' ,
        height: '70vh',
        position: 'relative',
        display: 'flex',
        flexWrap: 'wrap',
        boxShadow: '0 4px 8px 0 rgba(0,0,0,0.2)',
        transition: '0.3s',
        margin: '5px'     
    },
    detailList: { padding: '10px'},
    yellow: [{ color: '#ffaa44'}, iconClass],
    green: [{ color: '#498205' }, iconClass],
    red: [{ color: '#d13438' }, iconClass],
    blue: [{ color: '#0078d4' }, iconClass]
});

export default class Repositories extends React.Component<{ isAuthenticated: boolean, title: string }, IRepositoryState> {
    private allItems: IRepositoryItem[];

    constructor(props: { isAuthenticated: boolean, title: string }) {
        super(props);

        this.allItems = [];
        const columns: IColumn[] = [
            {
                key: 'name', name: 'Name', fieldName: 'name', minWidth: 200, maxWidth: 300, isRowHeader: true,
                isResizable: true, isSorted: false, isSortedDescending: false, onColumnClick: this.onColumnClick
            },
            {
                key: 'login', name: 'Owner', fieldName: 'admins', minWidth: 150, maxWidth: 200,
                isResizable: true, onColumnClick: this.onColumnClick, isMultiline:true
            },
            {
                key: 'status', name: 'Status', fieldName: 'repositoryStatus', minWidth: 100, maxWidth: 150,
                isResizable: true, onColumnClick: this.onColumnClick
            },
            {
                key: 'pullRequestCount', name: 'Open pull requests', fieldName: 'pullRequests', minWidth: 100,
                maxWidth: 150, isResizable: true, onColumnClick: this.onColumnClick
            },
            {
                key: 'issueCount', name: 'Open issues', fieldName: 'issues', minWidth: 75, maxWidth: 100,
                isResizable: true, onColumnClick: this.onColumnClick
            },
            {
                key: 'forkCount', name: 'Forks', fieldName: 'forks', minWidth: 75, maxWidth: 100,
                isResizable: true, onColumnClick: this.onColumnClick
            },
            {
                key: 'starsCount', name: 'Stars', fieldName: 'stargazers', minWidth: 75, maxWidth: 100,
                isResizable: true, onColumnClick: this.onColumnClick
            },
            {
                key: 'viewCount', name: 'Views', fieldName: 'views', minWidth: 75, maxWidth: 100,
                isResizable: true, onColumnClick: this.onColumnClick
            },
            {
                key: 'language', name: 'Language', fieldName: 'language', minWidth: 75, maxWidth: 100,
                isResizable: true, onColumnClick: this.onColumnClick
            },
            {
                key: 'featureArea', name: 'Feature area', fieldName: 'featureArea', minWidth: 200, maxWidth: 300,
                isResizable: true, onColumnClick: this.onColumnClick, isMultiline: true
            }
        ];

        if (this.props.isAuthenticated) {
            columns.push({
                key: 'vulnerabilityAlertsCount', name: 'Security Alerts', fieldName: 'vulnerabilityAlerts',
                minWidth: 75, maxWidth: 100, isResizable: true, onColumnClick: this.onColumnClick
            });
        }

        this.state = {
            columns,
            items: this.allItems,
            isLoading: false
        };
    }

    public componentDidMount = () => {
        if (this.props.title === "samples") {
            this.fetchSamples();
        }
        else if (this.props.title === "sdks") {
            this.fetchSDKs();
        }
    }

    // fetching the samples data from the samples api
    public fetchSamples = async () => {
        this.setState({ isLoading: true });
        const token = await authService.getAccessToken();
        const response = await fetch('api/samples',
            {
                headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
            });
        const data = await response.json();
        this.allItems = data;
        this.setState(
            {
                items: this.allItems,
                isLoading: false
            });
    }

    //fetching the sdk data from the sdk api
    public fetchSDKs = async () => {
        this.setState({ isLoading: true });
        const token = await authService.getAccessToken();
        const response = await fetch('api/sdks',
            {
                headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
            });
        const data = await response.json();
        this.allItems = data;
        this.setState(
            {
                items: this.allItems,
                isLoading: false
            });
    }

    public render(): JSX.Element {
        const { columns, items, isLoading } = this.state;

        return (
            <Fabric>
                <PageTitle title={'List of ' + this.props.title} />
                <div className={classNames.wrapper}>
                    <ScrollablePane scrollbarVisibility={ScrollbarVisibility.auto}>
                        <Sticky stickyPosition={StickyPositionType.Header}>
                            <TextField
                                className={filterListClass}
                                label='Filter by name:'
                                onChange={this.onFilterName}
                                styles={{ root: { maxWidth: '300px' } }}
                            />
                        </Sticky>
                        <div className={classNames.detailList}>
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
        );
    }

    private getKey(item: any, index?: number): string {
        return item.key;
    }

    private onFilterName = (ev: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, text?: string | undefined): void => {
        this.setState({
            items: text ? this.allItems.filter(i => i.name.toLowerCase().indexOf(text) > -1) : this.allItems
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
    };
}

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

// rendering the language and service component within the details list
function renderItemColumn(item: IRepositoryItem, index: number | undefined, column: IColumn | undefined) {
    const col = column as IColumn;
    const name = item.name.toLowerCase();
    const ownerProfiles = item.ownerProfiles;
    const status = item.repositoryStatus;
    const language = item.language;
    const pullRequestCount = item.pullRequests.totalCount;
    const issueCount = item.issues.totalCount;
    const starsCount = item.stargazers.totalCount;
    const forkCount = item.forks.totalCount;
    const views = item.views;
    const url = item.url;
    const featureArea = item.featureArea;
    const vulnerabilityAlertsCount = item.vulnerabilityAlerts.totalCount;  

    switch (col.name) {
        case 'Name':
            return <div>
                <Link to={`/samples/${name}`} ><span>{name} </ span></Link>
            </div>;

        case 'Owner':            
            return displayAdmins(ownerProfiles);            

        case 'Status':
            return checkStatus(status);

        case 'Language':
            return <span>{language}</span>;

        case 'Open pull requests':
            return <a href={`${url}/pulls`} target="_blank" rel="noopener noreferrer"> <span>{pullRequestCount} </span></a>

        case 'Open issues':
            return <a href={`${url}/issues`} target="_blank" rel="noopener noreferrer"> <span>{issueCount}</span></a>

        case 'Forks':
            return <a href={`${url}/network/members`} target="_blank" rel="noopener noreferrer"> <span>{forkCount} </span></a>;

        case 'Stars':
            return <a href={`${url}/stargazers`} target="_blank" rel="noopener noreferrer"> <FontIcon iconName="FavoriteStarFill" className={classNames.yellow} /><span>{starsCount} </span></a>;

        case 'Views':
            return <a href={`${url}/graphs/traffic`} target="_blank" rel="noopener noreferrer"> <span>{views} </span></a>;

        case 'Feature area':
            return <span> {featureArea} </span>;

        case 'Security Alerts':
            if (vulnerabilityAlertsCount > 0) {
                return <a href={`${url}/network/alerts`} target="_blank" rel="noopener noreferrer">
                    <FontIcon iconName="WarningSolid" className={classNames.yellow} /> <span>{vulnerabilityAlertsCount} </span></a>;
            }
            return <span>{vulnerabilityAlertsCount} </span>;

    }
}
function copyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
    const key = columnKey as keyof T;
    let itemsSorted = items.slice(0).sort((a: T, b: T) => (compare(a[key], b[key], isSortedDescending)));
    return itemsSorted;
}

function displayAdmins(ownerProfiles: any)
{
    if (ownerProfiles != null) {
        var div = document.createElement('div');

        for (var key in ownerProfiles) {
            var value = ownerProfiles[key];
            var user = document.createElement('a');
            user.href = value;
            user.target = '_blank';
            user.rel = 'noopener noreferrer';
            user.text = key.concat(', ');

            div.appendChild(user);
        }

        return <div dangerouslySetInnerHTML={{ __html: div.innerHTML }}></div>;
    }
    else
        return <span>{}</span>;
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
    }else {
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

function checkStatus(status: number) {
    switch (status) {
        case 0:
            return <TooltipHost content="Unknown" id={'Unknown'}>
                <span><FontIcon iconName="StatusCircleQuestionMark" className={classNames.blue} /> Unknown </span>
            </TooltipHost>;

        case 1:
            return <TooltipHost content="All dependencies in this repository are up to date" id={'UptoDate'}>
                <span><FontIcon iconName="CompletedSolid" className={classNames.green} /> Up To Date </span>
            </TooltipHost>;

        case 2:
            return <TooltipHost content="At least 1 dependency in this repository has a major/minor release update" id={'Update'}>
                <span><FontIcon iconName="WarningSolid" className={classNames.yellow} /> Update </span>
            </TooltipHost>;

        case 3:
            return <TooltipHost content="At least 1 dependency in this repository has a patch release update" id={'UrgentUpdate'}>
                <span><FontIcon iconName="StatusErrorFull" className={classNames.red} /> Urgent Update </span>
            </TooltipHost>;
    }

}


