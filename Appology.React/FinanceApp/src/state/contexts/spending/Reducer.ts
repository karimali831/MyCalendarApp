
import ISpendingState, { SpendingState } from './ISpendingState';
import { Reducer } from 'redux';
import { SpendingActions, SpendingActionTypes } from './Actions';

const SpendingReducer: Reducer<ISpendingState, SpendingActions> =
    (state = SpendingState.intialState, action) => {
        switch (action.type) {
            case SpendingActionTypes.LoadSpendingsSuccess:
                return { ...state, 
                    ...{ 
                        loading: false,
                        spendings: action.spendings,
                        catId: state.catId,
                        dateFilter: state.dateFilter,
                        isFinance: state.isFinance,
                        isSecondCat: state.isSecondCat
                    } 
                };

            default:
                return state;
        }
    }

export default SpendingReducer;