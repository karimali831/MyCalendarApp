
import { ICustomer } from 'src/models/ICustomer';
import { rootUrl } from '../components/utils/Utils';

export class AppologyApi {
    public rootUrl: string = `${rootUrl}/api`;

    public customers = async (): Promise<ICustomerResponse> => {
        return fetch(`${this.rootUrl}/errandrunner/customers`, {
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
        .then(data => data as ICustomerResponse);
    }
}

export const api = new AppologyApi();




export interface ICustomerResponse {
    customers: ICustomer[]
}
