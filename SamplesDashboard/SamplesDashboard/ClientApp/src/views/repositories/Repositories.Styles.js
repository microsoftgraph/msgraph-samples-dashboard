"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var Styling_1 = require("office-ui-fabric-react/lib/Styling");
exports.filterListClass = Styling_1.mergeStyles({
    display: 'block',
    padding: '10px'
});
exports.iconClass = Styling_1.mergeStyles({
    fontSize: 15,
    height: 15,
    width: 15,
    margin: '0 5px'
});
exports.classNames = Styling_1.mergeStyleSets({
    wrapper: {
        background: '#fff',
        height: '70vh',
        position: 'relative',
        display: 'flex',
        flexWrap: 'wrap',
        boxShadow: '0 4px 8px 0 rgba(0,0,0,0.2)',
        transition: '0.3s',
        margin: '5px'
    },
    detailList: { padding: '10px' },
    yellow: [{ color: '#ffaa44' }, exports.iconClass],
    green: [{ color: '#498205' }, exports.iconClass],
    red: [{ color: '#d13438' }, exports.iconClass],
    blue: [{ color: '#0078d4' }, exports.iconClass]
});
//# sourceMappingURL=Repositories.Styles.js.map