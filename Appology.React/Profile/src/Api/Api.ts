import { rootUrl } from 'src/components/utils/Utils';
import { IModel } from 'src/models/IModel';

export class Api {
    public rootUrl: string = `${rootUrl}/api/errandrunner`;

    public dummy = async (filter: string): Promise<IDummyResponse> => {
        return fetch(`${this.rootUrl}/dummy/${filter}`, {
            method: "GET",
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin'
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as IDummyResponse);
    }

    public post = async (model: IModel): Promise<boolean> => {
        return fetch(`${this.rootUrl}/stakeholders/register`, {
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
            return response.json();

        })
        .then(data => data as boolean);
    }

}

export const api = new Api();

export interface IDummyResponse {
    status: boolean
    data: IModel[]
}

