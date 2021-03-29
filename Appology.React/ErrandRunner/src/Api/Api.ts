
import IBaseModel from '@appology/react-components/src/SelectionRefinement/IBaseModel';
import { rootUrl } from 'src/components/utils/Utils';
import { Stakeholders } from 'src/Enums/Stakeholders';
import { IOrder } from 'src/models/IOrder';
import { IPlace } from 'src/models/IPlace';
import { IStakeholder } from 'src/models/IStakeholder';
import { ITrip } from 'src/models/ITrip';

export class Api {
    public rootUrl: string = `${rootUrl}/api/errandrunner`;

    public stakeholders = async (stakeholderId: Stakeholders, filter?: string): Promise<IStakeholderResponse> => {
        return fetch(`${this.rootUrl}/stakeholders/${stakeholderId}/${filter !== undefined ? filter : ""}`, {
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
        .then(data => data as IStakeholderResponse);
    }

    public registerStakeholder = async (stakeholder: IStakeholder): Promise<IStakeholderRegisterResponse> => {
        return fetch(`${this.rootUrl}/stakeholders/register`, {
            method: "POST",
            body: JSON.stringify(stakeholder),
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
        .then(data => data as IStakeholderRegisterResponse);
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

    public orders = async (userId: string): Promise<IOrdersResponse> => {
        return fetch(`${this.rootUrl}/orders/${userId}`, {
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
        .then(data => data as IOrdersResponse);
    }

    public order = async (orderId: string): Promise<IOrderResponse> => {
        return fetch(`${this.rootUrl}/order/${orderId}`, {
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
        .then(data => data as IOrderResponse);
    }

    public saveOrder = async (request: ISaveOrderRequest): Promise<ISaveOrderResponse> => {
        return fetch(`${this.rootUrl}/saveorder`, {
            method: "POST",
            body: JSON.stringify(request),
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
        .then(data => data as ISaveOrderResponse);
    }

    public deleteOrder = async (orderId: string): Promise<boolean> => {
        return fetch(`${this.rootUrl}/deleteorder/${orderId}`, {
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
        .then(data => data as boolean);
    }

    public paymentCharge = async (request: IPaymentChargeRequest): Promise<boolean> => {
        return fetch(`${this.rootUrl}/payment`, {
            method: "POST",
            body: JSON.stringify(request),
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

    public place = async (placeId: string): Promise<IPlace | null> => {
        return fetch(`${this.rootUrl}/place/${placeId}`, {
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
        .then(data => data as IPlace);
    }
}

export const api = new Api();

export interface IStakeholderResponse {
    stakeholders: IStakeholder[]
}

export interface IOrderResponse {
    trip: ITrip,
    order: IOrder,
    driver: IStakeholder,
    status: boolean
}

export interface ISaveOrderResponse {
    order: IOrder,
    trip: ITrip,
    status: boolean
}

export interface ISaveOrderRequest {
    order: IOrder,
    trip: ITrip
}

export interface IOrdersResponse {
    orders: IBaseModel[]
}

export interface IServicesResponse {
    services: IBaseModel[]
}

export interface IStakeholderRegisterResponse {
    stakeholder: IStakeholder | null,
    message: string
}

export interface IPaymentChargeRequest {
    customerId: string,
    amount: number,
    number: string,
    cvc: string,
    expMonth: number,
    expYear: number
}
