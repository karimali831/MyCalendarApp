
import ICustomersState, { CustomerState } from './ICustomerState';
import { Reducer } from 'redux';
import { LandingActions, LandingActionTypes } from './Actions';

const CustomerReducer: Reducer<ICustomersState, LandingActions> =
    (state = CustomerState.intialState, action) => {
        switch (action.type) {
            case LandingActionTypes.LoadCustomersSuccess:
                return {
                    ...state,
                    ...{
                        loading: false
                    }
                }

            case LandingActionTypes.FilterChanged:
                return { ...state, ...{ categoryFilter: action.filter } }   

            default:
                return state;
        }
    }

export default CustomerReducer;