// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import Repository from './Repository';
import { UpdateStatus } from './UpdateStatus';

export interface UpdateStats {
  totalUnknown: number;
  totalUpToDate: number;
  totalPatchUpdates: number;
  totalMinorUpdates: number;
  totalMajorUpdates: number;
  totalSecurityAlerts: number;
  percentUnknown: number;
  percentUpToDate: number;
  percentPatchUpdates: number;
  percentMinorUpdates: number;
  percentMajorUpdates: number;
  percentSecurityAlerts: number;
}

export function getRepositoryStats(repositories: Repository[]): UpdateStats {
  const stats: UpdateStats = {
    totalUnknown: 0,
    totalUpToDate: 0,
    totalPatchUpdates: 0,
    totalMinorUpdates: 0,
    totalMajorUpdates: 0,
    totalSecurityAlerts: 0,
    percentUnknown: 0,
    percentUpToDate: 0,
    percentPatchUpdates: 0,
    percentMinorUpdates: 0,
    percentMajorUpdates: 0,
    percentSecurityAlerts: 0,
  };

  for (const repository of repositories) {
    switch (repository.repositoryStatus) {
      case UpdateStatus.unknown:
        stats.totalUnknown++;
        break;
      case UpdateStatus.upToDate:
        stats.totalUpToDate++;
        break;
      case UpdateStatus.patchUpdate:
        stats.totalPatchUpdates++;
        break;
      case UpdateStatus.minorUpdate:
        stats.totalMinorUpdates++;
        break;
      case UpdateStatus.majorUpdate:
        stats.totalMajorUpdates++;
        break;
      case UpdateStatus.securityAlert:
        stats.totalSecurityAlerts++;
    }
  }

  calculatePercentages(stats, false);
  return stats;
}

export function getDependencyStats(repository: Repository): UpdateStats {
  const stats: UpdateStats = {
    totalUnknown: 0,
    totalUpToDate: 0,
    totalPatchUpdates: 0,
    totalMinorUpdates: 0,
    totalMajorUpdates: 0,
    totalSecurityAlerts: 0,
    percentUnknown: 0,
    percentUpToDate: 0,
    percentPatchUpdates: 0,
    percentMinorUpdates: 0,
    percentMajorUpdates: 0,
    percentSecurityAlerts: 0,
  };

  if (repository.dependencies) {
    for (const dependency of repository.dependencies!) {
      switch (dependency.status) {
        case UpdateStatus.unknown:
          stats.totalUnknown++;
          break;
        case UpdateStatus.upToDate:
          stats.totalUpToDate++;
          break;
        case UpdateStatus.patchUpdate:
          stats.totalPatchUpdates++;
          break;
        case UpdateStatus.minorUpdate:
          stats.totalMinorUpdates++;
          break;
        case UpdateStatus.majorUpdate:
          stats.totalMajorUpdates++;
          break;
        case UpdateStatus.securityAlert:
          stats.totalSecurityAlerts++;
      }
    }

    calculatePercentages(stats, true);
  }

  return stats;
}

function calculatePercentages(
  stats: UpdateStats,
  includeUnknowns: boolean
): void {
  // If unknowns are not included, remove them from the total
  // so percentages are accurate

  const total =
    stats.totalUpToDate +
    stats.totalPatchUpdates +
    stats.totalMinorUpdates +
    stats.totalMajorUpdates +
    stats.totalSecurityAlerts +
    (includeUnknowns ? stats.totalUnknown : 0);

  stats.percentUnknown = parseFloat(
    ((stats.totalUnknown / total) * 100).toFixed(1)
  );
  stats.percentUpToDate = parseFloat(
    ((stats.totalUpToDate / total) * 100).toFixed(1)
  );
  stats.percentPatchUpdates = parseFloat(
    ((stats.totalPatchUpdates / total) * 100).toFixed(1)
  );
  stats.percentMinorUpdates = parseFloat(
    ((stats.totalMinorUpdates / total) * 100).toFixed(1)
  );
  stats.percentMajorUpdates = parseFloat(
    ((stats.totalMajorUpdates / total) * 100).toFixed(1)
  );
  stats.percentSecurityAlerts = parseFloat(
    ((stats.totalSecurityAlerts / total) * 100).toFixed(1)
  );
}
