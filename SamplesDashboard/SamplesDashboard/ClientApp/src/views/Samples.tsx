import { Announced } from 'office-ui-fabric-react/lib/Announced';
import { DetailsList, DetailsListLayoutMode, IColumn, 
    Selection, SelectionMode } from 'office-ui-fabric-react/lib/DetailsList';
import { Fabric } from 'office-ui-fabric-react/lib/Fabric';
import { initializeIcons } from 'office-ui-fabric-react/lib/Icons';
import { MarqueeSelection } from 'office-ui-fabric-react/lib/MarqueeSelection';
import { ScrollablePane, ScrollbarVisibility } from 'office-ui-fabric-react/lib/ScrollablePane';
import { Sticky, StickyPositionType } from 'office-ui-fabric-react/lib/Sticky';
import { mergeStyles, mergeStyleSets } from 'office-ui-fabric-react/lib/Styling';
import { TextField } from 'office-ui-fabric-react/lib/TextField';
import * as React from 'react';
import { Link } from 'react-router-dom';

import Language from '../components/samples/Language';
import Service from '../components/samples/Service';
import { ISampleItem } from '../types/samples';

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
    }
});

export interface IListDataState {
    columns: IColumn[];
    items: ISampleItem[];
    selectionDetails: string;
    announcedMessage?: string;
}

export default class SampleList extends React.Component<{}, IListDataState> {
    private selection: Selection;
    private allItems: ISampleItem[];

    constructor(props: {}) {
        super(props);

        this.selection = new Selection({
            onSelectionChanged: () => this.setState({ selectionDetails: this.getSelectionDetails() })
        });
        // Populate with items for demos.
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
            { key: 'pullRequestCount', name: 'Open Pull Requests', fieldName: 'pullRequestCount', minWidth: 100, 
                maxWidth: 150, isResizable: true, onColumnClick: this.onColumnClick },
            { key: 'issueCount', name: 'Open Issues', fieldName: 'issueCount', minWidth: 100, maxWidth: 150, 
                isResizable: true, onColumnClick: this.onColumnClick },
            { key: 'starsCount', name: 'Stars', fieldName: 'starsCount', minWidth: 100, maxWidth: 100, 
                isResizable: true, onColumnClick: this.onColumnClick },
            { key: 'featureArea', name: 'Feature Area', fieldName: 'featureArea', minWidth: 100, maxWidth: 200, 
                isResizable: true, onColumnClick: this.onColumnClick },
            { key: 'vulnerabilityAlertsCount', name: 'Security Alerts', fieldName: 'vulnerabilityAlertsCount', 
                minWidth: 100, maxWidth: 100, isResizable: true, onColumnClick: this.onColumnClick }
        ];

        this.state = {
            columns,
            items: this.allItems,
            selectionDetails: this.getSelectionDetails(),
            announcedMessage: undefined
        };
    }

    public componentDidMount = () => {
        this.fetchData();
    }

    // fetching the data from the api
    public fetchData = async () => {
        const response = await fetch('api/Samples');
        const data = await response.json();
        this.allItems = data;
        this.setState(
            {
                items: data,
                selectionDetails: this.getSelectionDetails()
            });
    }

    public render(): JSX.Element {
        const { columns, items, selectionDetails, announcedMessage } = this.state;

        return (
            <Fabric>
                <div className={classNames.wrapper}>
                    <div className={detailListClass}>{selectionDetails}</div>
                    <ScrollablePane scrollbarVisibility={ScrollbarVisibility.auto}>
                        <Sticky stickyPosition={StickyPositionType.Header}>
                            <TextField
                                className={detailListClass}
                                label='Filter by name:'
                                onChange={this.onFilterName}
                                styles={{ root: { maxWidth: '300px' } }}
                            />
                        </Sticky>
                        {announcedMessage ? <Announced message={announcedMessage} /> : undefined}
                        <MarqueeSelection selection={this.selection} data-is-scrollable={true}>
                            <DetailsList
                                items={items}
                                columns={columns}
                                selectionMode={SelectionMode.none}
                                getKey={this.getKey}
                                setKey='multiple'
                                layoutMode={DetailsListLayoutMode.justified}
                                isHeaderVisible={true}
                                selection={this.selection}
                                selectionPreservedOnEmptyClick={true}
                                ariaLabelForSelectionColumn='Toggle selection'
                                ariaLabelForSelectAllCheckbox='Toggle selection for all items'
                                checkButtonAriaLabel='Row checkbox'
                                onRenderItemColumn={renderItemColumn}
                            />
                        </MarqueeSelection>
                    </ScrollablePane>
                </div>
            </Fabric>
        );
    }

    private getKey(item: any, index?: number): string {
        return item.key;
    }

    private getSelectionDetails(): string {
        const selectionCount = this.selection.getSelectedCount();

        switch (selectionCount) {
            case 0:
                return 'No items selected';
            case 1:
                return '1 item selected: ' + (this.selection.getSelection()[0] as ISampleItem).name;
            default:
                return `${selectionCount} items selected`;
        }
    }

    private onFilterName = (ev: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, 
        text?: string | undefined): void => {
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
    const sampleName = item[col.fieldName = 'name' as keyof ISampleItem] as string;
    const owner = item[col.fieldName = 'login' as keyof ISampleItem] as string;
    const status = item[col.fieldName = 'status' as keyof ISampleItem] as string;
    const pullRequestCount = item[col.fieldName = 'pullRequestCount' as keyof ISampleItem] as string;
    const issueCount = item[col.fieldName = 'issueCount' as keyof ISampleItem] as string;
    const starsCount = item[col.fieldName = 'starsCount' as keyof ISampleItem] as string;
    const vulnerabilityAlertsCount = item[col.fieldName = 'vulnerabilityAlertsCount' as keyof ISampleItem] as string;

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
            return <span><Language sampleName={sampleName} /></span>;

        case 'Open Pull Requests':
            return <span>{pullRequestCount} </span>;

        case 'Open Issues':
            return <span>{issueCount}</span>;

        case 'Stars':
            return <span>{starsCount} </span>;

        case 'Feature Area':
            return <span><Service sampleName={sampleName} /></span>;

        case 'Security Alerts':
            return <span>{vulnerabilityAlertsCount} </span>;

    }
}
function copyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
    const key = columnKey as keyof T;
    return items.slice(0).sort((a: T, b: T) => ((isSortedDescending ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1));
}
