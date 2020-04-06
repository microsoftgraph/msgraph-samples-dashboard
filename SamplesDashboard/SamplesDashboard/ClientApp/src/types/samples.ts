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
    url: any;
    featureArea: string;
    vulnerabilityAlerts: any;
    ownerUrl: string;

   
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
}
