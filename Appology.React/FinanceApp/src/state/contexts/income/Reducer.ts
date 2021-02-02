import { IncomeActions, IncomeActionTypes } from "./Actions";
import IIncomeState, { IncomeState } from './IIncomeState';
import { Reducer } from 'redux';

const IncomeReducer: Reducer<IIncomeState, IncomeActions> =
    (state = IncomeState.intialState, action) => {
        switch (action.type) {
            case IncomeActionTypes.LoadIncomesSuccess:
                return { ...state, 
                    ...{ 
                        loading: false,
                        incomes: action.incomes,
                        sourceId: state.sourceId,
                        dateFilter: state.dateFilter
                    } 
                };

            default:
                return state;
        }
    }

export default IncomeReducer;