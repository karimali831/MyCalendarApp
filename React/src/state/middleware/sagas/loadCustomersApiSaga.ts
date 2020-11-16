import { api, ICustomerResponse } from '../../../Api/Api'
import { call, put, takeLatest } from 'redux-saga/effects';
import { ReportErrorAction } from '../../contexts/error/Actions';
import { LandingSummaryActionTypes, LoadCustomersSuccessAction, LoadCustomersFailureAction } from '../../contexts/landing/Actions';

export default function* loadCustomersApiSaga() {
    yield takeLatest(LandingSummaryActionTypes.LoadCustomers, LoadCustomers);
}

export function* LoadCustomers() {
    try {
        
        // Start the API call asynchronously
        const result: ICustomerResponse = yield call(api.customers);

        // Create an action to dispatch on success with the returned entity from API
        const resultAction = new LoadCustomersSuccessAction(result.customers);

        // Dispatch the new action with Redux
        yield put(resultAction);
        
    } catch (e) {

        // Dispatch a failure action to Redux
        yield put(new LoadCustomersFailureAction(e.message));
        yield put(new ReportErrorAction(e.message));
        return;
    }
}