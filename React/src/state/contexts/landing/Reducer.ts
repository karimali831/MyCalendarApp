
import ICustomersState, { CustomerState } from './ICustomerState';
import { Reducer } from 'redux';
import { LandingSummaryActions, LandingSummaryActionTypes } from './Actions';

const CustomerReducer: Reducer<ICustomersState, LandingSummaryActions> =
    (state = CustomerState.intialState, action) => {
        switch (action.type) {
            case LandingSummaryActionTypes.LoadCustomersSuccess:
                return {
                    ...state,
                    ...{
                        loading: false
                    }
                }
                

            default:
                return state;
        }
    }

export default CustomerReducer;