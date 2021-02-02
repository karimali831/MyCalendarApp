
import { IIncomeResponse, api, IIncomeRequest } from '../../../Api/Api'
import { select, call, put, takeLatest } from 'redux-saga/effects';
import { ReportErrorAction } from '../../contexts/error/Actions';
import { getIncomeRequest } from 'src/state/contexts/income/Selectors';
import { IncomeActionTypes, LoadIncomesSuccessAction, LoadIncomesFailureAction } from 'src/state/contexts/income/Actions';

export default function* loadIncomesApiSaga() {
    yield takeLatest(IncomeActionTypes.LoadIncomes, loadIncomesRequest);
}

export function* loadIncomesRequest() {
    const request: IIncomeRequest = yield select(getIncomeRequest);
    yield call(loadIncomes, request);
}

export function* loadIncomes(request: IIncomeRequest) {
    try {
        // Start the API call asynchronously
        const result: IIncomeResponse = yield call(api.incomes, request);

        // Create an action to dispatch on success with the returned entity from API
        const resultAction = new LoadIncomesSuccessAction(result.incomes);

        // Dispatch the new action with Redux
        yield put(resultAction);
        
    } catch (e) {

        // Dispatch a failure action to Redux
        yield put(new LoadIncomesFailureAction(e.message));
        yield put(new ReportErrorAction(e.message));
        return;
    }
}