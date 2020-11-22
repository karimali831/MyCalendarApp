import { ICustomer } from './ICustomer';

export interface IAddressFinder {
    formData: ICustomer;
    postcodeAddresses: string[];
    loadingAddresses: boolean;
}

export interface IAddress {
    id: string[],
    label: string[]
}