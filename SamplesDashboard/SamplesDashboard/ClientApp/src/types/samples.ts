import { IColumn } from 'office-ui-fabric-react/lib/DetailsList';

export interface ISampleItem {
    key: number;
    name: string;
    owner: any;
    status: string;
    language: string;
    pullRequests: any;
    issues: any;
    stargazers: any;
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
    nodes:any;
    status: string;
}

export interface ISamplesState {
    columns: IColumn[];
    items: ISampleItem[];
    isLoading: boolean;
}
