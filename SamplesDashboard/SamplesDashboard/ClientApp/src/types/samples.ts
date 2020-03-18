import { IColumn } from 'office-ui-fabric-react/lib/DetailsList';

export interface IRepositoryItem {
    key: number;
    name: string;
    owner: any;
    repositoryStatus: any;
    language: string;
    pullRequests: any;
    issues: any;
    stargazers: any;
    forks: any;
    url: string;
    featureArea: string;
    vulnerabilityAlerts: any;
}
export interface IDetailsItem {
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
}
