import { IModel } from "src/models/IModel";

export default interface ILandingState {
    filter: string,
    dummy: IModel[]
}

export class LandingState {
    public static readonly intialState = {
        filter: "",
        dummy: []
    }
}