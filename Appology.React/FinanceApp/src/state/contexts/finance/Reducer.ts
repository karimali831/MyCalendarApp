import { FinanceActions, FinanceActionTypes } from "./Actions";
import IFinanceState, { FinanceState } from './IFinanceState';
import { Reducer } from 'redux';

const FinanceReducer: Reducer<IFinanceState, FinanceActions> =
    (state = FinanceState.intialState, action) => {
        switch (action.type) {
            case FinanceActionTypes.Finance:
                return { ...state, ...{ Finance: action.FinanceMessage } };

            default:
                return state;
        }
    }

export default FinanceReducer;