import { api, IDummyResponse} from '../../../Api/Api'
import { call, put, select, takeLatest } from 'redux-saga/effects';
import { ReportErrorAction } from '../../contexts/error/Actions';
import { LandingActionTypes, LoadDummySuccessAction, LoadDummyFailureAction } from '../../contexts/landing/Actions';
import { getFilter } from 'src/state/contexts/landing/Selectors';
export default function* loadDummyApiSaga() {
    yield takeLatest(LandingActionTypes.SelectedAction, loadDummyRequest);
}

export function* loadDummyRequest() {
    const filter: string = yield select(getFilter);

    yield call(loadDummy, filter);
}

export function* loadDummy(filter: string) {
    try {

        // Start the API call asynchronously
        const result: IDummyResponse = yield call(api.dummy, filter);

        // Create an action to dispatch on success with the returned entity from API
        const resultAction = new LoadDummySuccessAction(result.data);

        // Dispatch the new action with Redux
        yield put(resultAction);
        
    } catch (e) {

        // Dispatch a failure action to Redux
        yield put(new LoadDummyFailureAction(e.message));
        yield put(new ReportErrorAction(e.message));
        return;
    }
}