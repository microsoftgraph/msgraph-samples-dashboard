import { IColumn } from 'office-ui-fabric-react/lib/DetailsList';

export interface IRepositoryItem {
    key: number;
    name: string;
    ownerProfiles: any[];
    repositoryStatus: any;
    language: string;
    pullRequests: any;
    issues: any;
    stargazers: any;
    views: number;
    forks: any;
    url: string;
    featureArea: string;
    vulnerabilityAlerts: any;
    lastUpdated: any;
    identityStatus: any;
    graphStatus: any;
}

export interface IDetailsItem {
    azureSdkVersion: any;
    repository: any;
    packageName: string;
    requirements: string;
    currentVersion: any;
    releases: any;
    nodes: any;
    status: number;
    url: string;
    latestVersion: string;
}

export interface IRepositoryState {
    columns: IColumn[];
    items: IRepositoryItem[];
    isLoading: boolean;
    totalUptoDate: number;
    totalPatchUpdate: number;
    totalMinorUpdate: number;
    totalMajorUpdate: number;
    totalUrgentUpdate: number;
    uptoDatePercent: number;
    patchUpdatePercent: number;
    minorUpdatePercent: number;
    majorUpdatePercent: number;
    urgentUpdatePercent: number;
}
