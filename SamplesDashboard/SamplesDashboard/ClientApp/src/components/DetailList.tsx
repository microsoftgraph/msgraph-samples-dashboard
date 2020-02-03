import * as React from 'react';
import { TextField } from 'office-ui-fabric-react/lib/TextField';
import { DetailsList, DetailsListLayoutMode, Selection, IColumn, SelectionMode } from 'office-ui-fabric-react/lib/DetailsList';
import { MarqueeSelection } from 'office-ui-fabric-react/lib/MarqueeSelection';
import { ScrollablePane, ScrollbarVisibility } from 'office-ui-fabric-react/lib/ScrollablePane';
import { Sticky, StickyPositionType } from 'office-ui-fabric-react/lib/Sticky';
import { Fabric } from 'office-ui-fabric-react/lib/Fabric';
import { mergeStyles, mergeStyleSets } from 'office-ui-fabric-react/lib/Styling';
import { Announced } from 'office-ui-fabric-react/lib/Announced';
import { initializeIcons } from 'office-ui-fabric-react/lib/Icons';

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

export interface IListDataItem {
    key: number;
    name: string;
    owner: string;
    status: string;
    language: string;
    pullRequests: number;
    issues: number;
    stars: number;
    featureArea: string;
    securityAlerts: string;
}

export interface IListDataState {
    columns: IColumn[];
    items: IListDataItem[];
    selectionDetails: string;
    announcedMessage?: string;
}

export default class SampleList extends React.Component<{}, IListDataState> {
    private _selection: Selection;
    private _allItems: IListDataItem[];

    constructor(props: {}) {
        super(props);

        this._selection = new Selection({
        onSelectionChanged: () => this.setState({ selectionDetails: this._getSelectionDetails() })
        });
        // Populate with items for demos.
        this._allItems = [];
        const columns: IColumn[] = [
            { key: 'column1', name: 'Name', fieldName: 'name', minWidth: 200, maxWidth: 300, isRowHeader: true, isResizable: true, isSorted: true, isSortedDescending: false, onColumnClick: this._onColumnClick },
            { key: 'column2', name: 'Owner', fieldName: 'owner', minWidth: 100, maxWidth: 150, isResizable: true, onColumnClick: this._onColumnClick },
            { key: 'column3', name: 'Status', fieldName: 'status', minWidth: 100, maxWidth: 100, isResizable: true, onColumnClick: this._onColumnClick },
            { key: 'column4', name: 'Language', fieldName: 'language', minWidth: 100, maxWidth: 100, isResizable: true, onColumnClick: this._onColumnClick },
            { key: 'column5', name: 'Open Pull Requests', fieldName: 'pullRequests', minWidth: 150, maxWidth: 200, isResizable: true, onColumnClick: this._onColumnClick },
            { key: 'column6', name: 'Open Issues', fieldName: 'issues', minWidth: 100, maxWidth: 150, isResizable: true, onColumnClick: this._onColumnClick },
            { key: 'column7', name: 'Stars', fieldName: 'stars', minWidth: 100, maxWidth: 100, isResizable: true, onColumnClick: this._onColumnClick },
            { key: 'column8', name: 'Feature Area', fieldName: 'featureArea', minWidth: 100, maxWidth: 150, isResizable: true, onColumnClick: this._onColumnClick },
            { key: 'column9', name: 'Security Alerts', fieldName: 'securityAlerts', minWidth: 100, maxWidth: 150, isResizable: true, onColumnClick: this._onColumnClick }
        ];

        this.state = {
            columns: columns,
            items: this._allItems,
            selectionDetails: this._getSelectionDetails(),
            announcedMessage: undefined
        };
    }

    componentDidMount = () => {
        this.fetchData();
    }

    //fetching the data from the api
    fetchData = async () => {
        const response = await fetch('api/Samples');
        const data = await response.json();
        this._allItems = data;
        this.setState(
            {
                items: data,
                selectionDetails: this._getSelectionDetails()
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
                        label="Filter by name:"
                        onChange={this._onFilterName}
                  styles={{ root: { maxWidth: '300px' } }}
                    />
                    </Sticky>
                    {announcedMessage ? <Announced message={announcedMessage} /> : undefined}
                <MarqueeSelection selection={this._selection} data-is-scrollable={true}>
                <DetailsList
                    items={items}
                            columns={columns}
                            selectionMode={SelectionMode.none}
                    getKey={this._getKey}
                    setKey="multiple"
                    layoutMode={DetailsListLayoutMode.justified}
                    isHeaderVisible={true}
                    selection={this._selection}
                    selectionPreservedOnEmptyClick={true}
                    ariaLabelForSelectionColumn="Toggle selection"
                    ariaLabelForSelectAllCheckbox="Toggle selection for all items"
                    checkButtonAriaLabel="Row checkbox"
                    onItemInvoked={this._onItemInvoked}
                  />
                </MarqueeSelection>
                </ScrollablePane>
                </div>
        </Fabric>
        );
    }

    private _getKey(item: any, index?: number): string {
        return item.key;
    }

    private _getSelectionDetails(): string {
        const selectionCount = this._selection.getSelectedCount();

    switch (selectionCount) {
        case 0:
            return 'No items selected';
        case 1:
            return '1 item selected: ' + (this._selection.getSelection()[0] as IListDataItem).name;
        default:
            return `${selectionCount} items selected`;
        }
    }

    private _onFilterName = (ev: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, text?: string | undefined): void => {
        this.setState({
            items: text ? this._allItems.filter(i => i.name.toLowerCase().indexOf(text) > -1) : this._allItems
        });
    };

    private _onItemInvoked = (item: IListDataItem): void => {
        alert(`Item invoked: ${item.name}`);
    };

    private _onColumnClick = (ev: React.MouseEvent<HTMLElement>, column: IColumn): void => {
        const { columns, items } = this.state;
        const newColumns: IColumn[] = columns.slice();
        const currColumn: IColumn = newColumns.filter(currCol => column.key === currCol.key)[0];
        newColumns.forEach((newCol: IColumn) => {
            if (newCol === currColumn) {
                currColumn.isSortedDescending = !currColumn.isSortedDescending;
                currColumn.isSorted = true;
                this.setState({
                    announcedMessage: `${currColumn.name} is sorted ${currColumn.isSortedDescending ? 'descending' : 'ascending'}`
                });
            } else {
                newCol.isSorted = false;
                newCol.isSortedDescending = true;
            }
        });
        const newItems = _copyAndSort(items, currColumn.fieldName!, currColumn.isSortedDescending);
        this.setState({
            columns: newColumns,
            items: newItems
        });
    };
}
function _copyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
    const key = columnKey as keyof T;
    return items.slice(0).sort((a: T, b: T) => ((isSortedDescending ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1));
}
