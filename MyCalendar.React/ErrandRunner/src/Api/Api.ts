
import IBaseModel from '@appology/react-components/src/SelectionRefinement/IBaseModel';
import { rootUrl } from 'src/components/utils/Utils';
import { ICustomer } from 'src/models/ICustomer';

export class Api {
    public rootUrl: string = `${rootUrl}/api/errandrunner`;

    public customers = async (filter: string): Promise<ICustomerResponse> => {
        return fetch(`${this.rootUrl}/customers/${filter}`, {
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
        .then(data => data as ICustomerResponse);
    }

    public registerCustomer = async (customer: ICustomer): Promise<ICustomerRegisterResponse> => {
        return fetch(`${this.rootUrl}/customers/register`, {
            method: "POST",
            body: JSON.stringify(customer),
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
        .then(data => data as ICustomerRegisterResponse);
    }

    public services = async (): Promise<IServicesResponse> => {
        return fetch(`${this.rootUrl}/services`, {
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
        .then(data => data as IServicesResponse);
    }
}

export const api = new Api();

export interface ICustomerResponse {
    customers: ICustomer[]
}

export interface IServicesResponse {
    services: IBaseModel[]
}

export interface ICustomerRegisterResponse {
    customer: ICustomer | null,
    message: string
}