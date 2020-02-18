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
    featureArea: string;
    vulnerabilityAlerts: any;
}
export interface IDetailsItem {
    packageName: string;
    requirements: string;
    currentVersion: string;
    status: string;
}

export interface ISamplesState {
    columns: IColumn[];
    items: ISampleItem[];
    isLoading: boolean;
}