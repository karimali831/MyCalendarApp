import { SpendingActionTypes, LoadSpendingsFailureAction, LoadSpendingsSuccessAction } from './../../contexts/spending/Actions';
import { ISpendingRequest, ISpendingResponse, api } from '../../../Api/Api'
import { select, call, put, takeLatest } from 'redux-saga/effects';
import { ReportErrorAction } from '../../../state/contexts/error/Actions';
import { getSpendingsRequest } from 'src/state/contexts/spending/Selectors';

export default function* loadSpendingsApiSaga() {
    yield takeLatest(SpendingActionTypes.LoadSpendings, loadSpendingsRequest);
}

export function* loadSpendingsRequest() {
    const request: ISpendingRequest = yield select(getSpendingsRequest);
    yield call(loadSpendings, request);
}

export function* loadSpendings(request: ISpendingRequest) {
    try {

        // Start the API call asynchronously
        const result: ISpendingResponse = yield call(api.spendings, request);

        // Create an action to dispatch on success with the returned entity from API
        const resultAction = new LoadSpendingsSuccessAction(result.spendings);

        // Dispatch the new action with Redux
        yield put(resultAction);
        
    } catch (e) {

        // Dispatch a failure action to Redux
        yield put(new LoadSpendingsFailureAction(e.message));
        yield put(new ReportErrorAction(e.message));
        return;
    }
}