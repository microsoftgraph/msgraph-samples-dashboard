// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Header names for columns in repo list
export const repoListColumns = {
  name: 'Name',
  owners: 'Owners',
  status: 'Status',
  lastUpdated: 'Last updated',
  identityLibs: 'Identity libs',
  graphSdks: 'Graph SDKs',
  securityAlerts: 'Security alerts',
  openPRs: 'Open PRs',
  openIssues: 'Open issues',
  forks: 'Forks',
  stars: 'Stars',
  views: 'Views',
  language: 'Language',
  featureArea: 'Feature area',
};

export const dependencyListColumns = {
  packageName: 'Dependency',
  currentVersion: 'Current version',
  latestVersion: 'Latest version',
  status: 'Status',
};

// Status labels to display
export const statusLabels = [
  'None found',
  'Up to date',
  'Patch update',
  'Minor update',
  'Major update',
  'Urgent update',
];

// Tooltips for the statuses
export const repoStatusToolTips = [
  'No %OBJECT%s were found in this repository.',
  'All %OBJECT%s in this repository are up to date.',
  'At least one %OBJECT% in this repository has a patch update.',
  'At least one %OBJECT% in this repository has a minor update.',
  'At least one %OBJECT% in this repository has a major update.',
  'At least one %OBJECT% in this repository has a security alert update. Please visit GitHub to update.',
];

export const dependencyStatusToolTips = [
  'Could not determine the status of this dependency.',
  'This dependency is up to date.',
  'This dependency has a patch update.',
  'This dependency has a minor update.',
  'This dependency has a major update.',
  'This dependency has a security alert update. Please visit GitHub to update.',
];

// Special label/tooltip for unknown status
export const unknownStatus = {
  label: 'Unknown',
  toolTip: 'Could not parse dependencies. See Wiki for details.',
};
