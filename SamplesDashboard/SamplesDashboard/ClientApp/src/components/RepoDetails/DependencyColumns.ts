// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { IColumn } from '@fluentui/react';
import { dependencyListColumns } from '../../strings/Strings';

// Defines the columns in a repository list
export const dependencyColumns: IColumn[] = [
  {
    key: 'packageName',
    name: dependencyListColumns.packageName,
    fieldName: 'packageName',
    minWidth: 300,
    maxWidth: 400,
    isRowHeader: true,
    isResizable: true,
    isSorted: true,
    isSortedDescending: false,
  },
  {
    key: 'currentVersion',
    name: dependencyListColumns.currentVersion,
    fieldName: 'currentVersion',
    minWidth: 200,
    maxWidth: 300,
    isResizable: true,
  },
  {
    key: 'latestVersion',
    name: dependencyListColumns.latestVersion,
    fieldName: 'latestVersion',
    minWidth: 200,
    maxWidth: 300,
    isResizable: true,
  },
  {
    key: 'status',
    name: dependencyListColumns.status,
    fieldName: 'status',
    minWidth: 200,
    maxWidth: 300,
    isResizable: true,
  },
];
