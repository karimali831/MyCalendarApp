import IBaseModel from "@appology/react-components/src/SelectionRefinement/IBaseModel";
import { Stakeholders } from "src/Enums/Stakeholders";

export interface IStakeholder {
    id: string,
    stakeholderId: Stakeholders
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
    contactNo2: string,
    apiLat?: number,
    apiLng?: number,
    apiFormattedAddress: string
}

export interface IStakeholderSearch extends IBaseModel {
    stakeholders?: IStakeholder[],
    stakeholder: IStakeholder
}