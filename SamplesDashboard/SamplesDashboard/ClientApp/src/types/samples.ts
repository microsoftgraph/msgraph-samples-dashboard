import { IColumn } from 'office-ui-fabric-react/lib/DetailsList';

export interface ISampleItem {
    key: number;
    name: string;
    owner: string;
    status: string;
    language: string;
    pullRequests: number;
    issues: number;
    stars: number;
    featureArea: string;
    securityAlerts: string;
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