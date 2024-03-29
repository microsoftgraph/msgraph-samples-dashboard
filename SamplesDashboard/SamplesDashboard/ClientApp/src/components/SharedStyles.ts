// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { mergeStyles, mergeStyleSets } from '@fluentui/react';

export const filterListClass = mergeStyles({
  display: 'block',
  padding: '10px',
});

export const iconClass = mergeStyles({
  fontSize: 15,
  height: 15,
  width: 15,
  margin: '0 5px',
  verticalAlign: 'sub',
});

export const classNames = mergeStyleSets({
  wrapper: {
    height: '65vh',
    position: 'relative',
    display: 'flex',
    flexWrap: 'wrap',
    boxShadow: '0 4px 8px 0 rgba(0,0,0,0.2)',
    transition: '0.3s',
    margin: '5px',
  },
  detailList: { padding: '10px' },
  green: [{ color: '#498205' }, iconClass],
  yellowGreen: [{ color: '#8cbd18' }, iconClass],
  yellow: [{ color: '#ffaa44' }, iconClass],
  orange: [{ color: '#fd7e14' }, iconClass],
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
    margin: '5px',
  },
  yellowText: { color: '#ffaa44' },
  yellowGreenText: { color: '#8cbd18' },
  greenText: { color: '#498205' },
  orangeText: { color: '#fd7e14' },
  redText: { color: '#d13438' },
  blueText: { color: '#0078d4' },
});
