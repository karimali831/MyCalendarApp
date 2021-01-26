import { api, IStakeholderResponse } from '../../../Api/Api'
import { call, put, select, takeLatest } from 'redux-saga/effects';
import { ReportErrorAction } from '../../contexts/error/Actions';
import { LandingActionTypes, LoadStakeholdersSuccessAction, LoadStakeholdersFailureAction } from '../../contexts/landing/Actions';
import { getStakeholderId, getStakeholderSearchFilter } from 'src/state/contexts/landing/Selectors';
import { Stakeholders } from 'src/Enums/Stakeholders';

export default function* loadStakeholdersApiSaga() {
    yield takeLatest(LandingActionTypes.LoadStakeholders, loadStakeholdersRequest);
}

export function* loadStakeholdersRequest() {
    const filter: string = yield select(getStakeholderSearchFilter);
    const stakeholderId: Stakeholders = yield(select(getStakeholderId))

    yield call(loadStakeholders, filter, stakeholderId);
}

export function* loadStakeholders(filter: string, stakeholderId: Stakeholders) {
    try {

        // Start the API call asynchronously
        const result: IStakeholderResponse = yield call(api.stakeholders, stakeholderId, filter);

        // Create an action to dispatch on success with the returned entity from API
        const resultAction = new LoadStakeholdersSuccessAction(result.stakeholders);

        // Dispatch the new action with Redux
        yield put(resultAction);
        
    } catch (e) {

        // Dispatch a failure action to Redux
        yield put(new LoadStakeholdersFailureAction(e.message));
        yield put(new ReportErrorAction(e.message));
        return;
    }
}