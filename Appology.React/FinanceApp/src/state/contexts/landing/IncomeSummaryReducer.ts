
import IIncomeSummaryState, { IncomeSummaryState } from './IIncomeSummaryState';
import { Reducer } from 'redux';
import { LandingSummaryActions, LandingSummaryActionTypes } from './Actions';

const IncomeSummaryReducer: Reducer<IIncomeSummaryState, LandingSummaryActions> =
    (state = IncomeSummaryState.intialState, action) => {
        switch (action.type) {
            case LandingSummaryActionTypes.LoadIncomeSummary:
                return { ...state, ...{ loading: true } };

            case LandingSummaryActionTypes.LoadIncomeSummarySuccess:
                return {
                    ...state,
                    ...{
                        incomeSummary: action.incomeSummary, 
                        totalIncome: action.totalIncome,
                        loading: false
                    }
                }
                
            case LandingSummaryActionTypes.ShowSecondCategoryIncomeSummary:
                return {
                    ...state, 
                        ...{ 
                            showSecondCatSummary: state.showSecondCatSummary === action.secondCat ? null : action.secondCat  
                        }
                }

            case LandingSummaryActionTypes.IncomeSummaryDateFilterChange:
                    return { ...state, ...{ dateFilter: action.filter } };

            default:
                return state;
        }
    }

export default IncomeSummaryReducer;