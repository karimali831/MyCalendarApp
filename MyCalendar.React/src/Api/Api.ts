
import { ICustomer } from 'src/models/ICustomer';
import { rootUrl } from '../components/utils/Utils';

export class AppologyApi {
    public rootUrl: string = `${rootUrl}/api`;

    public customers = async (filter: string): Promise<ICustomerResponse> => {
        return fetch(`${this.rootUrl}/errandrunner/customers/${filter}`, {
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
        return fetch(`${this.rootUrl}/errandrunner/customers/register`, {
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
}

export const api = new AppologyApi();

export interface ICustomerResponse {
    customers: ICustomer[]
}

export interface ICustomerRegisterResponse {
    customer: ICustomer | null,
    message: string
}