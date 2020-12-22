import ICustomerState, { CustomerState } from './contexts/landing/ICustomerState';
import { IRouteState, RouteState } from './contexts/router/IRouteState';


// this represents the state of the 'entire' application
// it should be composed of other state definitions which represent state 'contexts'
// managed by seperate reducers and have actions which alter those contexts

export default interface IStoreState {
    router: IRouteState,
    customer: ICustomerState,
}

export class StoreState {
    public static readonly initialState: IStoreState = {
        router: RouteState.initialState,
        customer: CustomerState.intialState
    };
}

