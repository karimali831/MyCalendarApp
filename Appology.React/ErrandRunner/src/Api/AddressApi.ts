
import { IAddress, IAddressPrediction } from 'src/models/IAddressFinder';

export class AddressApi {
    public addressSearch = async (filter: string): Promise<IAddressAutoCompleteResponse> => {
        const API_URL = `https://ws.postcoder.com/pcw/PCWJW-3LC9V-XZJPQ-8X4G7/autocomplete/v2/uk/${filter}/?format=json`;

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


    public getAddress = async (address: string, id: string): Promise<IAddress[]> => {
        const API_URL = `https://ws.postcoder.com/pcw/PCWJW-3LC9V-XZJPQ-8X4G7/address/uk/${address}?lines=2&udprn=${id}&format=json`;

        return fetch(API_URL, {
            method: "GET"
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as IAddress[]);
    }
}

export const addressApi = new AddressApi();

export interface IAddressAutoCompleteResponse {
    predictions: IAddressPrediction[]
}

