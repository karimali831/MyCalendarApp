import IBaseModel from "@appology/react-components/src/SelectionRefinement/IBaseModel";

export interface IPlace {
    id: string,
    placeId: string,
    description: string,
    apiUrl: string,
    allowManual: boolean
}

export interface IPlaceItemSearch extends IBaseModel {
    price: number,
    maxQuantity: number
}