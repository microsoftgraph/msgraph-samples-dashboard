import { mergeStyles, mergeStyleSets } from 'office-ui-fabric-react/lib/Styling';

export const iconClass = mergeStyles({
    fontSize: 15,
    height: 15,
    width: 15,
    margin: '0 5px'
});

export const buttonClass = mergeStyles({
    margin: '10px'
});

export const descriptionClass = mergeStyles({
    paddingLeft: '10px',
    paddingBottom: '10px'
});

export const linkClass = mergeStyles({
    color: '#fff',
    selectors: {
        '&:hover': {
            color: '#fff'
        }
    }
});

export const classNames = mergeStyleSets({
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
    yellow: [{ color: '#ffaa44' }, iconClass],
    green: [{ color: '#498205' }, iconClass],
    red: [{ color: '#d13438' }, iconClass],
    blue: [{ color: '#0078d4' }, iconClass]
});
