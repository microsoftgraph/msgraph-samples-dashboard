"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var __assign = (this && this.__assign) || function () {
    __assign = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign.apply(this, arguments);
};
Object.defineProperty(exports, "__esModule", { value: true });
var office_ui_fabric_react_1 = require("office-ui-fabric-react");
var Fabric_1 = require("office-ui-fabric-react/lib/Fabric");
var Icons_1 = require("office-ui-fabric-react/lib/Icons");
var ScrollablePane_1 = require("office-ui-fabric-react/lib/ScrollablePane");
var Sticky_1 = require("office-ui-fabric-react/lib/Sticky");
var Styling_1 = require("office-ui-fabric-react/lib/Styling");
var TextField_1 = require("office-ui-fabric-react/lib/TextField");
var React = require("react");
var react_router_dom_1 = require("react-router-dom");
var PageTitle_1 = require("../components/layout/PageTitle");
Icons_1.initializeIcons();
var detailListClass = Styling_1.mergeStyles({
    display: 'block',
    marginBottom: '10px'
});
var iconClass = Styling_1.mergeStyles({
    fontSize: 15,
    height: 15,
    width: 15,
    margin: '0 5px'
});
var classNames = Styling_1.mergeStyleSets({
    wrapper: {
        height: '80vh',
        position: 'relative',
        display: 'flex',
        flexWrap: 'wrap'
    },
    pageTitle: {
        fontSize: Styling_1.FontSizes.large
    },
    yellow: [{
            color: '#ffaa44',
            marginRight: '5px'
        }, iconClass],
    green: [{ color: '#498205' }, iconClass],
    red: [{ color: '#d13438' }, iconClass],
    blue: [{ color: '#0078d4' }, iconClass]
});
var SDKs = /** @class */ (function (_super) {
    __extends(SDKs, _super);
    function SDKs(props) {
        var _this = _super.call(this, props) || this;
        _this.onFilterName = function (ev, text) {
            _this.setState({
                items: text ? _this.allItems.filter(function (i) { return i.name.toLowerCase().indexOf(text) > -1; }) : _this.allItems
            });
        };
        _this.onColumnClick = function (ev, column) {
            var _a = _this.state, columns = _a.columns, items = _a.items;
            var newColumns = columns.slice();
            var currColumn = newColumns.filter(function (currCol) { return column.key === currCol.key; })[0];
            newColumns.forEach(function (newCol) {
                if (newCol === currColumn) {
                    currColumn.isSortedDescending = !currColumn.isSortedDescending;
                    currColumn.isSorted = true;
                }
                else {
                    newCol.isSorted = false;
                    newCol.isSortedDescending = true;
                }
            });
            var newItems = copyAndSort(items, currColumn.fieldName, currColumn.isSortedDescending);
            _this.setState({
                columns: newColumns,
                items: newItems
            });
        };
        _this.allItems = [];
        var columns = [
            {
                key: 'name', name: 'Name', fieldName: 'name', minWidth: 200, maxWidth: 300, isRowHeader: true,
                isResizable: true, isSorted: false, isSortedDescending: false, onColumnClick: _this.onColumnClick
            },
            {
                key: 'login', name: 'Owner', fieldName: 'login', minWidth: 75, maxWidth: 150,
                isResizable: true, onColumnClick: _this.onColumnClick
            },
            {
                key: 'status', name: 'Status', fieldName: 'sampleStatus', minWidth: 100, maxWidth: 150,
                isResizable: true, onColumnClick: _this.onColumnClick
            },
            {
                key: 'language', name: 'Language', fieldName: 'language', minWidth: 75, maxWidth: 100,
                isResizable: true, onColumnClick: _this.onColumnClick
            },
            {
                key: 'pullRequestCount', name: 'Open Pull Requests', fieldName: 'pullRequests', minWidth: 100,
                maxWidth: 150, isResizable: true, onColumnClick: _this.onColumnClick
            },
            {
                key: 'issueCount', name: 'Open Issues', fieldName: 'issues', minWidth: 75, maxWidth: 100,
                isResizable: true, onColumnClick: _this.onColumnClick
            },
            {
                key: 'forkCount', name: 'Forks', fieldName: 'forks', minWidth: 75, maxWidth: 100,
                isResizable: true, onColumnClick: _this.onColumnClick
            },
            {
                key: 'starsCount', name: 'Stars', fieldName: 'stargazers', minWidth: 75, maxWidth: 100,
                isResizable: true, onColumnClick: _this.onColumnClick
            },
            {
                key: 'featureArea', name: 'Feature Area', fieldName: 'featureArea', minWidth: 200, maxWidth: 300,
                isResizable: true, onColumnClick: _this.onColumnClick, isMultiline: true
            }
        ];
        if (_this.props.isAuthenticated) {
            columns.push({
                key: 'vulnerabilityAlertsCount', name: 'Security Alerts', fieldName: 'vulnerabilityAlerts',
                minWidth: 75, maxWidth: 100, isResizable: true, onColumnClick: _this.onColumnClick
            });
        }
        _this.state = {
            columns: columns,
            items: _this.allItems,
            isLoading: false
        };
        return _this;
    }
    // fetching the data from the api
    SDKs.prototype.render = function () {
        var _a = this.state, columns = _a.columns, items = _a.items, isLoading = _a.isLoading;
        return (React.createElement(Fabric_1.Fabric, null,
            React.createElement(PageTitle_1.default, { title: 'List of SDKs' }),
            React.createElement("div", { className: classNames.wrapper },
                React.createElement(ScrollablePane_1.ScrollablePane, { scrollbarVisibility: ScrollablePane_1.ScrollbarVisibility.auto },
                    React.createElement(Sticky_1.Sticky, { stickyPosition: Sticky_1.StickyPositionType.Header },
                        React.createElement(TextField_1.TextField, { className: detailListClass, label: 'Filter by name:', onChange: this.onFilterName, styles: { root: { maxWidth: '300px' } } })),
                    React.createElement(office_ui_fabric_react_1.ShimmeredDetailsList, { items: items, columns: columns, selectionMode: office_ui_fabric_react_1.SelectionMode.none, layoutMode: office_ui_fabric_react_1.DetailsListLayoutMode.justified, isHeaderVisible: true, onRenderItemColumn: renderItemColumn, enableShimmer: isLoading, onRenderDetailsHeader: onRenderDetailsHeader })))));
    };
    return SDKs;
}(React.Component));
exports.default = SDKs;
var onRenderDetailsHeader = function (props, defaultRender) {
    if (!props) {
        return null;
    }
    var onRenderColumnHeaderTooltip = function (tooltipHostProps) { return (React.createElement(office_ui_fabric_react_1.TooltipHost, __assign({}, tooltipHostProps))); };
    return (React.createElement(Sticky_1.Sticky, { stickyPosition: Sticky_1.StickyPositionType.Header, isScrollSynced: true }, defaultRender(__assign(__assign({}, props), { onRenderColumnHeaderTooltip: onRenderColumnHeaderTooltip }))));
};
// rendering items to their specific columns
function renderItemColumn(item, index, column) {
    var col = column;
    var sampleName = item.name;
    var owner = item.owner.login;
    var status = item.sampleStatus;
    var language = item.language;
    var pullRequestCount = item.pullRequests.totalCount;
    var issueCount = item.issues.totalCount;
    var starsCount = item.stargazers.totalCount;
    var forkCount = item.forks.totalCount;
    var url = item.url;
    var featureArea = item.featureArea;
    var vulnerabilityAlertsCount = item.vulnerabilityAlerts.totalCount;
    switch (col.name) {
        case 'Name':
            return React.createElement("div", null,
                React.createElement(react_router_dom_1.Link, { to: "/samples/" + sampleName },
                    React.createElement("span", null,
                        sampleName,
                        " ")));
        case 'Owner':
            return React.createElement("span", null,
                owner,
                " ");
        case 'Status':
            return checkStatus(status);
        case 'Language':
            return React.createElement("span", null, language);
        case 'Open Pull Requests':
            return React.createElement("a", { href: url + "/pulls", target: "_blank", rel: "noopener noreferrer" },
                " ",
                React.createElement("span", null,
                    pullRequestCount,
                    " "));
        case 'Open Issues':
            return React.createElement("a", { href: url + "/issues", target: "_blank", rel: "noopener noreferrer" },
                " ",
                React.createElement("span", null, issueCount));
        case 'Forks':
            return React.createElement("a", { href: url + "/network/members", target: "_blank", rel: "noopener noreferrer" },
                " ",
                React.createElement("span", null,
                    forkCount,
                    " "));
        case 'Stars':
            return React.createElement("a", { href: url + "/stargazers", target: "_blank", rel: "noopener noreferrer" },
                " ",
                React.createElement(office_ui_fabric_react_1.FontIcon, { iconName: "FavoriteStarFill", className: classNames.yellow }),
                React.createElement("span", null,
                    starsCount,
                    " "));
        case 'Feature Area':
            return React.createElement("span", null,
                " ",
                featureArea,
                " ");
        case 'Security Alerts':
            if (vulnerabilityAlertsCount > 0) {
                return React.createElement("a", { href: url + "/network/alerts", target: "_blank", rel: "noopener noreferrer" },
                    React.createElement(office_ui_fabric_react_1.FontIcon, { iconName: "WarningSolid", className: classNames.yellow }),
                    " ",
                    React.createElement("span", null,
                        vulnerabilityAlertsCount,
                        " "));
            }
            return React.createElement("span", null,
                vulnerabilityAlertsCount,
                " ");
    }
}
function copyAndSort(items, columnKey, isSortedDescending) {
    var key = columnKey;
    var itemsSorted = items.slice(0).sort(function (a, b) { return (compare(a[key], b[key], isSortedDescending)); });
    return itemsSorted;
}
function compare(a, b, isSortedDescending) {
    // Handle the possible scenario of blank inputs 
    // and keep them at the bottom of the lists
    if (!a)
        return 1;
    if (!b)
        return -1;
    var valueA;
    var valueB;
    var comparison = 0;
    if (typeof a === 'string' || a instanceof String) {
        // Use toUpperCase() to ignore character casing
        valueA = a.toUpperCase();
        valueB = b.toUpperCase();
        // its an item of type number
    }
    else if (typeof a == 'number' && typeof b == 'number') {
        valueA = a;
        valueB = b;
    }
    else {
        // its an object which has a totalCount property
        valueA = a.totalCount;
        valueB = b.totalCount;
    }
    if (valueA > valueB) {
        comparison = 1;
    }
    else if (valueA < valueB) {
        comparison = -1;
    }
    if (isSortedDescending) {
        comparison = comparison * -1;
    }
    return comparison;
}
function checkStatus(status) {
    switch (status) {
        case 0:
            return React.createElement("span", null,
                React.createElement(office_ui_fabric_react_1.FontIcon, { iconName: "StatusCircleQuestionMark", className: classNames.blue }),
                " Unknown ");
        case 1:
            return React.createElement("span", null,
                React.createElement(office_ui_fabric_react_1.FontIcon, { iconName: "CompletedSolid", className: classNames.green }),
                " Up To Date ");
        case 2:
            return React.createElement("span", null,
                React.createElement(office_ui_fabric_react_1.FontIcon, { iconName: "WarningSolid", className: classNames.yellow }),
                " Update ");
        case 3:
            return React.createElement("span", null,
                React.createElement(office_ui_fabric_react_1.FontIcon, { iconName: "StatusErrorFull", className: classNames.red }),
                " Urgent Update ");
    }
}
//# sourceMappingURL=SDKs.js.map