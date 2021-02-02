import { IGoogleGeoLocation } from "src/Api/GoogleApi";

export interface ITrip {
    tripId: string,
    assignedRunnerId: string,
    pickupId: string,
    pickupPlace: string,
    pickupLat: number,
    pickupLng: number,
    distance: string,
    duration: string,
    dropOffAddress: string,
    dropOffPostCode: string
}

export interface ITripOverview {
    tripId: string,
    pickupPlace: string,
    stakeholderLocation: IGoogleGeoLocation,
    pickupId: string,
    distance: string,
    duration: string,
    dropOffAddress: string,
    dropOffPostCode: string
}