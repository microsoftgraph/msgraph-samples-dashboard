"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.classNames = exports.linkClass = exports.descriptionClass = exports.buttonClass = exports.iconClass = void 0;
var react_1 = require("@fluentui/react");
exports.iconClass = react_1.mergeStyles({
    fontSize: 15,
    height: 15,
    width: 15,
    margin: '0 5px'
});
exports.buttonClass = react_1.mergeStyles({
    margin: '10px'
});
exports.descriptionClass = react_1.mergeStyles({
    paddingLeft: '10px',
    paddingBottom: '10px'
});
exports.linkClass = react_1.mergeStyles({
    color: '#fff',
    selectors: {
        '&:hover': {
            color: '#fff'
        }
    }
});
exports.classNames = react_1.mergeStyleSets({
    wrapper: {
        height: '55vh',
        position: 'relative',
        display: 'flex',
        flexWrap: 'wrap',
        boxShadow: '0 4px 8px 0 rgba(0,0,0,0.2)',
        transition: '0.3s',
        margin: '5px'
    },
    detailList: { padding: '10px' },
    green: [{ color: '#498205' }, exports.iconClass],
    yellowGreen: [{ color: '#8cbd18' }, exports.iconClass],
    yellow: [{ color: '#ffaa44' }, exports.iconClass],
    orange: [{ color: '#fd7e14' }, exports.iconClass],
    red: [{ color: '#d13438' }, exports.iconClass],
    blue: [{ color: '#0078d4' }, exports.iconClass]
});
//# sourceMappingURL=Details.Styles.js.map