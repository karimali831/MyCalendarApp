import { ICategoryResponse } from './Api';
import { CategoryType } from '../enums/CategoryType';
import { rootUrl } from '../components/utils/Utils';


export class CommonFinanceApi {

    public rootUrl: string = `${rootUrl}/api`;

    public add = async (model: any, route: string) => {
        return fetch(`${this.rootUrl}/${route}/add`, {
            method: "POST",
            body: JSON.stringify(model),
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin',
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
        })
    }

    public update = async (model: IUpdateRequest) => {
        return fetch(`${this.rootUrl}/update`, {
            method: "POST",
            body: JSON.stringify(model),
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin',
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
        })
    }

    public remove = async (id: number, table: string) => {
        return fetch(`${this.rootUrl}/delete/${id}/${table}`, {
            method: "POST",
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin',
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
        })
    }

    public categories = async (typeId?: CategoryType, catsWithSubs: boolean = false): Promise<ICategoryResponse> => {
        return fetch(`${this.rootUrl}/categories/${typeId}/${catsWithSubs}`, {
            method: "GET",
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin',
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(response.statusText);
                }
                return response.json();
            })
            .then(data => data as ICategoryResponse);
    }
}

export interface IUpdateRequest {
    table: string, 
    field: string, 
    value: any, 
    id: number
}

export const commonApi = new CommonFinanceApi();
