"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.classNames = exports.iconClass = exports.filterListClass = void 0;
var react_1 = require("@fluentui/react");
exports.filterListClass = react_1.mergeStyles({
    display: 'block',
    padding: '10px'
});
exports.iconClass = react_1.mergeStyles({
    fontSize: 15,
    height: 15,
    width: 15,
    margin: '0 5px',
    verticalAlign: 'sub'
});
exports.classNames = react_1.mergeStyleSets({
    wrapper: {
        background: '#fff',
        height: '58vh',
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
    blue: [{ color: '#0078d4' }, exports.iconClass],
    statsCard: {
        background: '#fff',
        height: '20vh',
        position: 'relative',
        display: 'flex',
        flexWrap: 'wrap',
        boxShadow: '0 4px 8px 0 rgba(0,0,0,0.2)',
        transition: '0.3s',
        margin: '5px'
    },
    yellowText: { color: '#ffaa44' },
    yellowGreenText: { color: '#8cbd18' },
    greenText: { color: '#498205' },
    orangeText: { color: '#fd7e14' },
    redText: { color: '#d13438' },
    blueText: { color: '#0078d4' }
});
//# sourceMappingURL=Repositories.Styles.js.map