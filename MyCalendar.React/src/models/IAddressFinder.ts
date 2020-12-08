import IBaseModel from 'src/components/SelectionRefinement/IBaseModel';
import { ICustomer } from './ICustomer';

export interface IAddressFinder {
    formData: ICustomer;
    postcodeAddresses: string[];
    loadingAddresses: boolean;
}

export interface IAddressLabel {
    id: string[],
    label: string[]
}

export interface IAddressSearch extends IBaseModel {
    addresses?: IAddressSugestion[]
}

export interface IAddressSugestion {
    address: string,
    url: string,
    id: string
}

export interface IAddress {
    postcode: string,
    latitude: string,
    longitude: string,
    building_number: string,
    line_1: string,
    line_2: string,
    line_3: string,
    line_4: string,
    town_or_city: string,
    county: string,
    country: string,
    residential: boolean
}

