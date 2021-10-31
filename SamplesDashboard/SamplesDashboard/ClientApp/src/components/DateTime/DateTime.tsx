// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { TooltipHost } from '@fluentui/react';

interface DateTimeProps {
  dateTimeOffset: string;
}

// Display a date/time string with the date showing,
// and the time in a tooltip
export default function DateTime(props: DateTimeProps) {
  const dateTime = new Date(props.dateTimeOffset);
  return (
    <TooltipHost content={dateTime.toTimeString()}>
      <span>{dateTime.toDateString()}</span>
    </TooltipHost>
  );
}
