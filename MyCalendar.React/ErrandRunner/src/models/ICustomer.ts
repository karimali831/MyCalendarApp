import IBaseModel from "@appology/react-components/src/SelectionRefinement/IBaseModel";

export interface ICustomer {
    custId: string,
    firstName: string,
    lastName: string,
    address1: string,
    address2: string,
    address3: string,
    town: string,
    county: string,
    country: string,
    postcode: string,
    contactNo1: string,
    contactNo2: string
}

export interface ICustomerSearch extends IBaseModel {
    customers?: ICustomer[],
    addressLine1: string,
    postCode: string
}