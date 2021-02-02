import { ChartActions, ChartActionTypes } from "./Actions";
import IChartState, { ChartState } from './ChartState';
import { Reducer } from 'redux';

const CWTLReducer: Reducer<IChartState, ChartActions> =
    (state = ChartState.intialState, action) => {
        switch (action.type) {
            case ChartActionTypes.LoadChart:
                return { ...state, 
                    ...{ 
                        request: action.request,
                        type: action.chartDataType
                    } 
                };
     
            case ChartActionTypes.LoadIncomeExpenseChartSuccess:
                return { ...state, incomeExpenseComparisonChart: action.chart, loading: false };

            case ChartActionTypes.LoadIncomesByCategoryChartSuccess:
                return { ...state, incomeCategoryComparisonChart: action.chart, loading: false  };

            case ChartActionTypes.LoadExpensesByCategoryChartSuccess:
                return { ...state, expenseCategoryComparisonChart: action.chart, loading: false  };

            case ChartActionTypes.LoadFinancesChartSuccess:
                return { ...state, financesComparisonChart: action.chart, loading: false  };

            default:
                return state;
        }
    }

export default CWTLReducer;