import { mergeStyles, mergeStyleSets } from 'office-ui-fabric-react/lib/Styling';

export const filterListClass = mergeStyles({
    display: 'block',
    padding: '10px'
});

export const iconClass = mergeStyles({
    fontSize: 15,
    height: 15,
    width: 15,
    margin: '0 5px'
});

export const classNames = mergeStyleSets({
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
    yellow: [{ color: '#ffaa44' }, iconClass],
    green: [{ color: '#498205' }, iconClass],
    red: [{ color: '#d13438' }, iconClass],
    blue: [{ color: '#0078d4' }, iconClass],
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
    yellowText: {color: '#ffaa44' },
    greenText: { color: '#498205' },
    redText: { color: '#d13438' },
    blueText: { color: '#0078d4' }
});