import { api, IMonthComparisonChartResponse } from '../../../Api/Api'
import { call, put, takeLatest, select } from 'redux-saga/effects';
import { ReportErrorAction } from '../../contexts/error/Actions';
import { spendingSummaryDateFilter, incomeSummaryDateFilter } from 'src/state/contexts/landing/Selectors';
import { getMonthComparisonChartRequest, getChartDataType } from 'src/state/contexts/chart/Selectors';
import { ChartActionTypes, LoadExpensesByCategoryChartSuccessAction, LoadIncomeExpenseChartSuccessAction, LoadIncomesByCategoryChartSuccessAction, LoadChartFailureAction, LoadFinancesChartSuccessAction } from 'src/state/contexts/chart/Actions';
import { ChartDataType } from 'src/enums/ChartType';
import { LoadIncomeSummaryAction, LoadSpendingSummaryAction } from 'src/state/contexts/landing/Actions';

export default function* loadChartApiSaga() {
    yield takeLatest(ChartActionTypes.LoadChart, loadChart)
}

export function* loadChart() {
    try {
        let dateFilter;
        let apiSubUrl : string = "";
        let monthlyComparisonChart : boolean = true;
        
        const type = yield select(getChartDataType);

        if (!type) {
            yield put(new LoadChartFailureAction("Chart datatype not yielded"));
            return;
        }
     
        switch (type) {
            case ChartDataType.IncomeSummary:
                monthlyComparisonChart = false;
                dateFilter = yield select(incomeSummaryDateFilter);
                yield put(new LoadIncomeSummaryAction(dateFilter))
                break;
    
            case ChartDataType.SpendingSummary:
                monthlyComparisonChart = false;
                dateFilter = yield select(spendingSummaryDateFilter);
                yield put(new LoadSpendingSummaryAction(dateFilter))
                break;

            case ChartDataType.SpendingSummaryByCategory:
                apiSubUrl = "spendings/chart"
                dateFilter = yield select(spendingSummaryDateFilter);
                break;
            case ChartDataType.IncomeExpenseSummary:
                apiSubUrl = "finances/chart/incomeexpense"
                dateFilter = yield select(incomeSummaryDateFilter);
                break;

            case ChartDataType.Finances:
                apiSubUrl = "finances/chart"
                dateFilter = yield select(spendingSummaryDateFilter);
                break;
                
            case ChartDataType.IncomeSummaryByCategory:
                apiSubUrl = "incomes/chart"
                dateFilter = yield select(incomeSummaryDateFilter);
                break;
            default:
                apiSubUrl = ""
                break;
        }

        if (monthlyComparisonChart === true) {
            const request = yield select(getMonthComparisonChartRequest, dateFilter);

            // Start the API call asynchronously
            const result: IMonthComparisonChartResponse = yield call(api.monthlyComparison, request, apiSubUrl);

            // Create an action to dispatch on success with the returned entity from API & Dispatch the new action with Redux
            switch (type) {
                case ChartDataType.SpendingSummaryByCategory:
                    yield put(new LoadExpensesByCategoryChartSuccessAction(result))
                    break;
                case ChartDataType.IncomeExpenseSummary:
                    yield put(new LoadIncomeExpenseChartSuccessAction(result))
                    break;
                case ChartDataType.IncomeSummaryByCategory:
                    yield put(new LoadIncomesByCategoryChartSuccessAction(result))
                    break;
                case ChartDataType.Finances:
                    yield put(new LoadFinancesChartSuccessAction(result))
                    break;
            }
        }
        
    } catch (e) {

        // Dispatch a failure action to Redux
        const type = yield select(getChartDataType);
        yield put(new LoadChartFailureAction(e.message + " - " + ChartDataType[type]));
        yield put(new ReportErrorAction(e.message));
        return;
    }
}