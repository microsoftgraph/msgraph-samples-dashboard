// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { FontIcon } from '@fluentui/react';

import './StatusCard.css';

interface StatusCardProps {
  iconClass: string;
  textClass: string;
  label: string;
  count: number;
  percentage: number;
}

export default function StatusCard(props: StatusCardProps) {
  return (
    <div className='col-sm-2'>
      <div className='card'>
        <div className='card-body'>
          <p className='card-text'>
            <FontIcon
              iconName='StatusCircleInner'
              className={props.iconClass}
            />
            {props.label}: {props.count}
          </p>
          <div className='card-footer'>
            <span className={props.textClass}>{props.percentage}%</span>
          </div>
        </div>
      </div>
    </div>
  );
}
