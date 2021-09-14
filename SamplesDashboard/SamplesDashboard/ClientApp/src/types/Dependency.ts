// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { UpdateStatus } from './UpdateStatus';

export default interface Dependency {
  packageName: string;
  currentVersion?: string;
  latestVersion?: string;
  status: UpdateStatus;
}
