import IProfileState, { ProfileState } from './contexts/profile/IState';
import { IRouteState, RouteState } from './contexts/router/IRouteState';


// this represents the state of the 'entire' application
// it should be composed of other state definitions which represent state 'contexts'
// managed by seperate reducers and have actions which alter those contexts

export default interface IStoreState {
    router: IRouteState,
    profile: IProfileState,
}

export class StoreState {
    public static readonly initialState: IStoreState = {
        router: RouteState.initialState,
        profile: ProfileState.intialState
    };
}

