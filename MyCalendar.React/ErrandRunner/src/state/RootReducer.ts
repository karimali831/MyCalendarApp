import IStoreState from './IStoreState';
import { combineReducers } from 'redux';
import CustomerReducer from './contexts/landing/Reducer';
import { connectRouter } from 'connected-react-router';
import { History } from 'history';


// Root reducer combining all other state reducers
export default
    (history: History<any>) =>
        combineReducers<IStoreState>({
            router: connectRouter(history),
            customer: CustomerReducer
        }); 