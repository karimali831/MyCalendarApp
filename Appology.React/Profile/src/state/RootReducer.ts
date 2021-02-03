import IStoreState from './IStoreState';
import { combineReducers } from 'redux';
import { connectRouter } from 'connected-react-router';
import { History } from 'history';
import LandingReducer from './contexts/landing/Reducer';


// Root reducer combining all other state reducers
export default
    (history: History<any>) =>
        combineReducers<IStoreState>({
            router: connectRouter(history),
            landing: LandingReducer
        }); 