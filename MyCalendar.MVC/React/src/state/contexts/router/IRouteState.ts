export interface IRouteState {
    action: string
    location: IRouteDetails
}

export interface IRouteDetails {
    pathname: string
    search: string
    hash: string
}

export class RouteState {
    public static readonly initialState = {
        action: "",
        location: {
            pathname: "",
            search: "",
            hash: ""
        }
    }
}
