
import IBaseModel from '@appology/react-components/dist/SelectionRefinement/IBaseModel';


export interface IAddressLabel {
    id: string[],
    label: string[]
}


export interface IAddressSearch extends IBaseModel {
    addresses?: IAddressPrediction[]
}


export interface IAddressPrediction{
    prediction: string,
    refs: string,
    complete: boolean
}

export interface IAddress {
    addressline1: string,
    summaryline: string,
    number: string,
    street: string,
    posttown: string,
    county: string,
    postcode: string
}