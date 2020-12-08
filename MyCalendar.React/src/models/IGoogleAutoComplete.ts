import IBaseModel from 'src/components/SelectionRefinement/IBaseModel';

export interface IGoogleAutoComplete{
    status: string,
    predictions: IPrediction[]
}

export interface IPrediction {
    place_id: string,
    description: string
}

export interface IGoogleAutoCompleteSearch extends IBaseModel {
    predictions?: IPrediction[]
}