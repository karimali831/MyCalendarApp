import { IGoogleGeoLocation } from "src/Api/GoogleApi";
import { IStakeholder } from "./IStakeholder";

export interface IGoogleMapMarker {
    driver?: IStakeholder,
    customer?: IStakeholder,
    centerMark?: IGoogleGeoLocation,
    selected: boolean,
    label: string
}
