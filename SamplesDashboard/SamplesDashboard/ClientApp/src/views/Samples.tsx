import {  DetailsListLayoutMode, IColumn,
    SelectionMode, ShimmeredDetailsList } from 'office-ui-fabric-react';
import { Fabric } from 'office-ui-fabric-react/lib/Fabric';
import { initializeIcons } from 'office-ui-fabric-react/lib/Icons';
import { ScrollablePane, ScrollbarVisibility } from 'office-ui-fabric-react/lib/ScrollablePane';
import { Sticky, StickyPositionType } from 'office-ui-fabric-react/lib/Sticky';
import { FontSizes, mergeStyles, mergeStyleSets } from 'office-ui-fabric-react/lib/Styling';
import { TextField } from 'office-ui-fabric-react/lib/TextField';
import * as React from 'react';
import { Link } from 'react-router-dom';

import PageTitle from '../components/layout/PageTitle';
import { ISampleItem, ISamplesState } from '../types/samples';

initializeIcons();

const detailListClass = mergeStyles({
    display: 'block',
    marginBottom: '10px'
});

const classNames = mergeStyleSets({
    wrapper: {
        height: '80vh',
        position: 'relative',
        display: 'flex',
        flexWrap: 'wrap'
    },
    pageTitle: {
        fontSize: FontSizes.large
    }
});

export default class Samples extends React.Component<{}, ISamplesState> {
    private allItems: ISampleItem[];

    constructor(props: {}) {
        super(props);

        this.allItems = [];
        const columns: IColumn[] = [
            { key: 'name', name: 'Name', fieldName: 'name', minWidth: 200, maxWidth: 300, isRowHeader: true, 
                isResizable: true, isSorted: true, isSortedDescending: false, onColumnClick: this.onColumnClick },
            { key: 'login', name: 'Owner', fieldName: 'login', minWidth: 100, maxWidth: 150, 
                isResizable: true, onColumnClick: this.onColumnClick },
            { key: 'status', name: 'Status', fieldName: 'status', minWidth: 100, maxWidth: 150, 
                isResizable: true, onColumnClick: this.onColumnClick },
            { key: 'language', name: 'Language', fieldName: 'language', minWidth: 100, maxWidth: 150, 
                isResizable: true, onColumnClick: this.onColumnClick },
            { key: 'pullRequestCount', name: 'Open Pull Requests', fieldName: 'totalCount', minWidth: 100, 
                maxWidth: 150, isResizable: true, onColumnClick: this.onColumnClick },
            { key: 'issueCount', name: 'Open Issues', fieldName: 'totalCount', minWidth: 100, maxWidth: 150, 
                isResizable: true, onColumnClick: this.onColumnClick },
            { key: 'starsCount', name: 'Stars', fieldName: 'totalCount', minWidth: 100, maxWidth: 100, 
                isResizable: true, onColumnClick: this.onColumnClick },
            { key: 'featureArea', name: 'Feature Area', fieldName: 'featureArea', minWidth: 200, maxWidth: 300, 
                isResizable: true, onColumnClick: this.onColumnClick },
            { key: 'vulnerabilityAlertsCount', name: 'Security Alerts', fieldName: 'totalCount', 
                minWidth: 100, maxWidth: 100, isResizable: true, onColumnClick: this.onColumnClick }
        ];

        this.state = {
            columns,
            items: this.allItems,
            isLoading: false
        };
    }

    public componentDidMount = () => {
        this.fetchData();
    }

    // fetching the data from the api
    public fetchData = async () => {
        this.setState({ isLoading: true });
        const response = await fetch('api/samples');
        const data = await response.json();
        this.allItems = data;
        console.log(data);
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
                <PageTitle title='List of samples' />
                <div className={classNames.wrapper}>
                    <ScrollablePane scrollbarVisibility={ScrollbarVisibility.auto}>
                        <Sticky stickyPosition={StickyPositionType.Header}>
                            <TextField
                                className={detailListClass}
                                label='Filter by name:'
                                onChange={this.onFilterName}
                                styles={{ root: { maxWidth: '300px' } }}
                            />
                        </Sticky>
                            <ShimmeredDetailsList
                                items={items}
                                columns={columns}
                                selectionMode={SelectionMode.none}
                                layoutMode={DetailsListLayoutMode.justified}
                                isHeaderVisible={true}
                                onRenderItemColumn={renderItemColumn}
                                enableShimmer={isLoading}
                            />
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

// rendering the language and service component within the details list
function renderItemColumn(item: ISampleItem, index: number | undefined, column: IColumn | undefined) {
    const col = column as IColumn;
    const sampleName = item.name;   
    const owner = item.owner.login;
    const status = item.status;
    const language = item.language;
    const pullRequestCount = item.pullRequests.totalCount;
    const issueCount = item.issues.totalCount;
    const starsCount = item.stargazers.totalCount;
    const url = item.url;
    const featureArea = item.featureArea;
    const vulnerabilityAlertsCount = item.vulnerabilityAlerts.totalCount;

    switch (col.name) {
        case 'Name':
            return <div>
                <Link to={`/samples/${sampleName}`} ><span>{sampleName} </ span></Link>
            </div>;
        
        case 'Owner':
            return <span>{owner} </span>;

        case 'Status':
            return <span>{status} </span>;

        case 'Language':
            return <span>{language}</span>;

        case 'Open Pull Requests':
            return <a href={`${url}/pulls`} target="_blank"> <span>{pullRequestCount} </span></a>

        case 'Open Issues':
            return <a href={`${url}/issues`} target="_blank"> <span>{issueCount}</span></a>

        case 'Stars':
            return <a href={`${url}`} target="_blank"> <span>{starsCount} </span></a>;

        case 'Feature Area':
            return <span> {featureArea} </span>;

        case 'Security Alerts':
            if (vulnerabilityAlertsCount > 0) {
                return <a href={`${url}/network/alerts`} target="_blank"> <span>{vulnerabilityAlertsCount} </span></a>;
            }
            return <span>{vulnerabilityAlertsCount} </span>;

    }
}
function copyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
    const key = columnKey as keyof T;
    return items.slice(0).sort((a: T, b: T) => ((isSortedDescending ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1));
}
