// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { FontIcon, Link, TooltipHost } from '@fluentui/react';

import { classNames } from '../SharedStyles';
import { UpdateStatus } from '../../types/UpdateStatus';
import {
  dependencyStatusToolTips,
  statusLabels,
  repoStatusToolTips,
  unknownStatus,
} from '../../strings/Strings';

interface StatusIndicatorProps {
  // The description of the "thing" this status applies to
  // Will be used in tooltips
  objectName: string;
  status: number;
  // If true, the unknown status is displayed as "Unknown"
  // If false, the unknown status is displayed as "None found"
  useUnknown: boolean;
  mode: 'repo' | 'dependency';
}

function getToolTip(props: StatusIndicatorProps): string {
  if (
    props.status === UpdateStatus.unknown &&
    props.useUnknown &&
    props.mode === 'repo'
  ) {
    return unknownStatus.toolTip;
  }

  var replaceString = props.objectName;

  if (props.status <= UpdateStatus.upToDate && replaceString.endsWith('y')) {
    replaceString = replaceString.substr(0, replaceString.length - 1);
    replaceString += 'ie';
  }

  return props.mode === 'repo'
    ? repoStatusToolTips[props.status].replace('%OBJECT%', replaceString)
    : dependencyStatusToolTips[props.status];
}

function getIconClass(status: number): string {
  switch (status) {
    case UpdateStatus.upToDate:
      return classNames.green;
    case UpdateStatus.patchUpdate:
      return classNames.yellowGreen;
    case UpdateStatus.minorUpdate:
      return classNames.yellow;
    case UpdateStatus.majorUpdate:
      return classNames.orange;
    case UpdateStatus.securityAlert:
      return classNames.red;
    default:
      return classNames.blue;
  }
}

function getStatusLabel(status: number, useUnknown: boolean): string {
  if (status === UpdateStatus.unknown && useUnknown) {
    return unknownStatus.label;
  }

  return statusLabels[status];
}

export default function StatusIndicator(props: StatusIndicatorProps) {
  return (
    <TooltipHost content={getToolTip(props)}>
      <span className='me-1'>
        <FontIcon
          iconName={
            props.status === UpdateStatus.unknown && props.useUnknown
              ? 'StatusCircleQuestionMark'
              : 'StatusCircleInner'
          }
          className={`${getIconClass(props.status)} me-0`}
        />{' '}
        {getStatusLabel(props.status, props.useUnknown)}
      </span>
      {props.status === UpdateStatus.unknown &&
        props.useUnknown &&
        props.mode === 'repo' && (
          <Link
            target='_blank'
            rel='noreferrer'
            href='https://github.com/microsoftgraph/msgraph-samples-dashboard/wiki/DevX-Dashboard-overview'
          >
            Wiki
          </Link>
        )}
    </TooltipHost>
  );
}
