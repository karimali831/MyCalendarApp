
import { rootUrl } from 'src/components/utils/Utils';
import { IAddress, IAddressSugestion } from 'src/models/IAddressFinder';
import { ICustomer } from 'src/models/ICustomer';

export class Api {
    public rootUrl: string = `${rootUrl}/api/errandrunner`;
    public getAddressApiKey = "TAbHcgudWkeBaSM31zj-Mg29276";
    public googleApiKey = "AIzaSyCkhI2-jqWmsoU83gO19VQHovXxeiCnr4I";
    public googleApiKey2 = "AIzaSyCLwS2RBnIr0LFPvyRrMzrJCQGNbYLEcUY";

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

    public googleAutoComplete = async (filter: string): Promise<IGoogleAutoCompleteResponse> => {
        const proxyurl = "https://cors-anywhere.herokuapp.com/";
        const API_URL = `https://maps.googleapis.com/maps/api/place/autocomplete/json?input=${filter}&types=establishment&location=51.5556640625,0.008618839085102081&radius=5000&strictbounds&key=${this.googleApiKey}`;

        return fetch(proxyurl + API_URL, {
            method: "GET"
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as IGoogleAutoCompleteResponse);
    }

    public addressSearch = async (filter: string): Promise<IAddressAutoCompleteResponse> => {
        const API_URL = `https://api.getaddress.io/autocomplete/${filter}?api-key=${this.getAddressApiKey}&top=20&all=true`;

        return fetch(API_URL, {
            method: "GET"
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as IAddressAutoCompleteResponse);
    }

    public getAddress = async (id: string): Promise<IAddress> => {
        const API_URL = `https://api.getAddress.io/get/${id}?api-key=${this.getAddressApiKey}`;

        return fetch(API_URL, {
            method: "GET"
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as IAddress);
    }

    // 
    public addressSearch2 = async (filter: string): Promise<IAddressAutoCompleteResponse2> => {
        const API_URL = `https://ws.postcoder.com/pcw/PCWJW-3LC9V-XZJPQ-8X4G7/autocomplete/v2/uk/${filter}`;

        return fetch(API_URL, {
            method: "GET"
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as IAddressAutoCompleteResponse2);
    }

    public getAddress2 = async (address: string, id: string): Promise<IAddressResponse> => {
        const API_URL = `https://ws.postcoder.com/pcw/PCWJW-3LC9V-XZJPQ-8X4G7/address/uk/${address}?lines=2&udprn=${id}`;

        return fetch(API_URL, {
            method: "GET"
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as IAddressResponse);
    }
}

export const api = new Api();

export interface ICustomerResponse {
    customers: ICustomer[]
}

export interface ICustomerRegisterResponse {
    customer: ICustomer | null,
    message: string
}

export interface IGoogleAutoCompleteResponse {
    predictions: [],
    status: string
}

export interface IAddressAutoCompleteResponse {
    suggestions: IAddressSugestion[]
}
//
export interface IAddressAutoCompleteResponse2 {
    autoComplete: IAddressSugestion
}

export interface IAddressAutoComplete {
    predictions: IAddressPredictions
    status: boolean
}

export interface IAddressPredictions {
    prediction: IAddressPrediction[]
}

export interface IAddressPrediction {
    prediction: string,
    refs: number,
    complete: boolean
}

export interface IAddressResponse {
    addresses: IAddresses
}

export interface IAddresses {
    address: IAddress2
}

export interface IAddress2 {
    addressline1: string,
    summaryline: string,
    
}