// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import Dependency from './Dependency';
import RepoOwner from './RepoOwner';

export default interface Repository {
  name: string;
  description: string;
  owners: RepoOwner[];
  securityAlerts: number;
  issues: number;
  pullRequests: number;
  starGazers: number;
  views: number;
  forks: number;
  url: string;
  defaultBranch: string;
  lastUpdated: string;
  language: string;
  featureArea: string;
  repositoryStatus: number;
  identityStatus: number;
  graphStatus: number;
  dependencies?: Dependency[];
}
