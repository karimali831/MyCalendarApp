import { IGoogleGeoLocation } from "src/Api/GoogleApi";

export interface ITripOverview {
    pickupLocation: string,
    customerLocation: IGoogleGeoLocation,
    pickupId: string,
    distance: string,
    duration: string,
    dropOffAddress: string,
    dropOffPostCode: string
}