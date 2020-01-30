import * as React from 'react';
import { TextField } from 'office-ui-fabric-react/lib/TextField';
import { DetailsList, DetailsListLayoutMode, Selection, IColumn } from 'office-ui-fabric-react/lib/DetailsList';
import { MarqueeSelection } from 'office-ui-fabric-react/lib/MarqueeSelection';
import { Fabric } from 'office-ui-fabric-react/lib/Fabric';
import { mergeStyles } from 'office-ui-fabric-react/lib/Styling';

const exampleChildClass = mergeStyles({
    overflow: 'auto',
    display: 'block',
    marginBottom: '10px'
});

export interface IDetailsListBasicExampleItem {
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

export interface IDetailsListBasicExampleState {
    columns: IColumn[];
    items: IDetailsListBasicExampleItem[];
    selectionDetails: string;
}

export default class DetailsListBasicExample extends React.Component<{}, IDetailsListBasicExampleState> {
    private _selection: Selection;
    private _allItems: IDetailsListBasicExampleItem[];

    constructor(props: {}) {
        super(props);

        this._selection = new Selection({
        onSelectionChanged: () => this.setState({ selectionDetails: this._getSelectionDetails() })
        });
        // Populate with items for demos.
        this._allItems = [];
        const columns: IColumn[] = [
            { key: 'column1', name: 'Name', fieldName: 'name', minWidth: 100, maxWidth: 200, isRowHeader:true, isResizable: true, onColumnClick: this._onColumnClick },
            { key: 'column2', name: 'Owner', fieldName: 'owner', minWidth: 100, maxWidth: 200, isResizable: true, onColumnClick: this._onColumnClick },
            { key: 'column3', name: 'Status', fieldName: 'status', minWidth: 50, maxWidth: 200, isResizable: true, onColumnClick: this._onColumnClick },
            { key: 'column4', name: 'Language', fieldName: 'language', minWidth: 100, maxWidth: 200, isResizable: true, onColumnClick: this._onColumnClick },
            { key: 'column5', name: 'Open Pull Requests', fieldName: 'pullRequests', minWidth: 100, maxWidth: 200, isResizable: true, onColumnClick: this._onColumnClick },
            { key: 'column6', name: 'Open Issues', fieldName: 'issues', minWidth: 100, maxWidth: 200, isResizable: true, onColumnClick: this._onColumnClick },
            { key: 'column7', name: 'Stars', fieldName: 'stars', minWidth: 50, maxWidth: 200, isResizable: true, onColumnClick: this._onColumnClick },
            { key: 'column8', name: 'Feature Area', fieldName: 'featureArea', minWidth: 50, maxWidth: 200, isResizable: true, onColumnClick: this._onColumnClick },
            { key: 'column9', name: 'Security Alerts', fieldName: 'securityAlerts', minWidth: 100, maxWidth: 200, isResizable: true, onColumnClick: this._onColumnClick }
        ];

        this.state = {
            columns: columns,
            items: this._allItems,
            selectionDetails: this._getSelectionDetails()
        };
    }

    componentDidMount = () => {
        this.fetchData();
    }

    //fetching the data from the api
    fetchData = async () => {
        const response = await fetch('api/Samples');
        const data = await response.json();
        this.setState(
            {
                items: data,
                selectionDetails: this._getSelectionDetails()
            });
    }

    public render(): JSX.Element {
    const { columns, items, selectionDetails } = this.state;

    return (
        <Fabric>
            <div className={exampleChildClass}>{selectionDetails}</div>
            <div data-is-scrollable="true" >
                <TextField
                  className={exampleChildClass}
                        label="Filter by owner:"
                        onChange={this._onFilterOwner}
                  styles={{ root: { maxWidth: '300px' } }}
                />         
                <MarqueeSelection selection={this._selection} data-is-scrollable={true}>
                <DetailsList
                    data-is-scrollable="true"
                    items={items}
                    columns={columns}
                    setKey="set"
                    layoutMode={DetailsListLayoutMode.justified}
                    selection={this._selection}
                    selectionPreservedOnEmptyClick={true}
                    ariaLabelForSelectionColumn="Toggle selection"
                    ariaLabelForSelectAllCheckbox="Toggle selection for all items"
                    checkButtonAriaLabel="Row checkbox"
                    onItemInvoked={this._onItemInvoked}
                  />
                </MarqueeSelection>
            </div> 
      </Fabric>
    );
  }

  private _getSelectionDetails(): string {
    const selectionCount = this._selection.getSelectedCount();

    switch (selectionCount) {
      case 0:
        return 'No items selected';
      case 1:
        return '1 item selected: ' + (this._selection.getSelection()[0] as IDetailsListBasicExampleItem).name;
      default:
        return `${selectionCount} items selected`;
    }
  }

    private _onFilterOwner = (ev: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, text: string): void => {
        this.setState({
            items: text ? this._allItems.filter(i => i.owner.toLowerCase().indexOf(text) > -1) : this._allItems
        });
    };

    private _onItemInvoked = (item: IDetailsListBasicExampleItem): void => {
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
