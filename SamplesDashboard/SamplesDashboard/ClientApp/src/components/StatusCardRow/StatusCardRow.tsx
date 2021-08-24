// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import StatusCard from '../StatusCard/StatusCard';
import { UpdateStats } from '../../types/UpdateStats';
import { classNames } from '../SharedStyles';

interface StatusCardRowProps {
  stats?: UpdateStats;
  // Set to true if an additional card for items with
  // unknown status should be shown
  includeUnknowns: boolean;
}

export default function StatusCardRow(props: StatusCardRowProps) {
  return (
    <div className='row'>
      <StatusCard
        label='Up to date'
        count={props.stats?.totalUpToDate || 0}
        percentage={props.stats?.percentUpToDate || 0}
        iconClass={classNames.green}
        textClass={classNames.greenText}
      />
      <StatusCard
        label='Patch updates'
        count={props.stats?.totalPatchUpdates || 0}
        percentage={props.stats?.percentPatchUpdates || 0}
        iconClass={classNames.yellowGreen}
        textClass={classNames.yellowGreenText}
      />
      <StatusCard
        label='Minor updates'
        count={props.stats?.totalMinorUpdates || 0}
        percentage={props.stats?.percentMinorUpdates || 0}
        iconClass={classNames.yellow}
        textClass={classNames.yellowText}
      />
      <StatusCard
        label='Major updates'
        count={props.stats?.totalMajorUpdates || 0}
        percentage={props.stats?.percentMajorUpdates || 0}
        iconClass={classNames.orange}
        textClass={classNames.orangeText}
      />
      <StatusCard
        label='Security alerts'
        count={props.stats?.totalSecurityAlerts || 0}
        percentage={props.stats?.percentSecurityAlerts || 0}
        iconClass={classNames.red}
        textClass={classNames.redText}
      />
      {props.includeUnknowns && (
        <StatusCard
          label='Unknown versions'
          count={props.stats?.totalUnknown || 0}
          percentage={props.stats?.percentUnknown || 0}
          iconClass={classNames.blue}
          textClass={classNames.blueText}
        />
      )}
    </div>
  );
}
