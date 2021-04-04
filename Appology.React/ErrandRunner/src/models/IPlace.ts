import IBaseModel from "@appology/react-components/src/SelectionRefinement/IBaseModel";

export interface IPlace {
    id: string,
    serviceId: number,
    serviceName: string,
    placeId: string,
    name: string,
    description: string,
    apiProductUrl: string,
    apiTimeslotUrl: string,
    imagePath: string,
    allowManual: boolean,
    active: boolean,
    displayController: boolean
}

export interface IPlaceItemSearch extends IBaseModel {
    price: number,
    maxQuantity: number
}