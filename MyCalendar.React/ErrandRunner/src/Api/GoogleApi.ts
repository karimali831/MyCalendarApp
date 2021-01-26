
export class GoogleApi {
    public googlePlacesApiKey = "AIzaSyCuWUXrr2OdrVu6QVgxcfuIQrigSMBG4cY";
    public googleMapsApiKey = "AIzaSyCLwS2RBnIr0LFPvyRrMzrJCQGNbYLEcUY";
    public proxyurl = "https://boiling-peak-84695.herokuapp.com/";

    public googleAutoComplete = async (filter: string, lat: number, lng: number, radius: string): Promise<IGoogleAutoCompleteResponse> => {
        const API_URL = `https://maps.googleapis.com/maps/api/place/autocomplete/json?input=${filter}&types=establishment&location=${lat},${lng}&radius=${radius}&strictbounds&key=${this.googlePlacesApiKey}`;

        return fetch(this.proxyurl + API_URL, {
            method: "GET"
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as IGoogleAutoCompleteResponse);
    }

    public getAddressGeo = async (address: string): Promise<IGoogleAddressGeoResponse> => {
        const API_URL = `https://maps.googleapis.com/maps/api/geocode/json?address=${address}}&key=${this.googlePlacesApiKey}`;

        return fetch(API_URL, {
            method: "GET"
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as IGoogleAddressGeoResponse);
    }

    public distanceMatrix = async (pickupPlaceId: string, stakeholderLocation: IGoogleGeoLocation): Promise<IGoogleDistanceMatrixResponse> => {
        const API_URL = `https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=place_id:${pickupPlaceId}&destinations=${stakeholderLocation.lat},${stakeholderLocation.lng}&key=${this.googlePlacesApiKey}`;

        return fetch(this.proxyurl + API_URL, {
            method: "GET"
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as IGoogleDistanceMatrixResponse);
    }

    public placeDetails = async (placeId: string): Promise<IGooglePlaceDetailsResponse> => {
        const API_URL = `https://maps.googleapis.com/maps/api/place/details/json?placeid=${placeId}&key=${this.googlePlacesApiKey}`;

        return fetch(this.proxyurl + API_URL, {
            method: "GET"
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as IGooglePlaceDetailsResponse);
    }
}

export const googleApi = new GoogleApi();

export interface IGooglePlaceDetailsResponse {
    result: IPlaceDetails
}

export interface IPlaceDetails {
    geometry: IPlaceDetailsGeometry
}

export interface IPlaceDetailsGeometry {
    location: IGoogleGeoLocation
}

export interface IGoogleAutoCompleteResponse {
    predictions: [],
    status: string
}

export interface IGoogleAddressGeoResponse {
    results: IGoogleGeo[],
    status: boolean
}

export interface IGoogleGeo {
    geometry: IGoogleGeometry
}

export interface IGoogleGeometry {
    location: IGoogleGeoLocation
}

export interface IGoogleGeoLocation {
    lat: number,
    lng: number
}

export interface IGoogleDistanceMatrixResponse {
    rows: IGoogleDistanceMatrixRows[],
    status: string
}

export interface IGoogleDistanceMatrixRows {
    elements: IGoogleDistanceMatrixElement[]

}

export interface IGoogleDistanceMatrixElement {
    distance: IGoogleDistanceMatrix,
    duration: IGoogleDistanceMatrix,
    status: string
}

export interface IGoogleDistanceMatrix {
    text: string,
    value: number
}