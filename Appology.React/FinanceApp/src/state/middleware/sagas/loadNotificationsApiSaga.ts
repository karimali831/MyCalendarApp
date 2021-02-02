import { api, INotificationResponse } from '../../../Api/Api'
import { call, put, takeLatest } from 'redux-saga/effects';
import { ReportErrorAction } from '../../contexts/error/Actions';
import { LandingSummaryActionTypes, LoadNotificationsSuccessAction, LoadNotificationsFailureAction } from '../../contexts/landing/Actions';

export default function* loadNotificationApiSaga() {
    yield takeLatest(LandingSummaryActionTypes.LoadNotifications, loadNotifications);
}

export function* loadNotifications() {
    try {
        
        // Start the API call asynchronously
        const result: INotificationResponse = yield call(api.notifications);

        // Create an action to dispatch on success with the returned entity from API
        const resultAction = new LoadNotificationsSuccessAction(result.notifications);

        // Dispatch the new action with Redux
        yield put(resultAction);
        
    } catch (e) {

        // Dispatch a failure action to Redux
        yield put(new LoadNotificationsFailureAction(e.message));
        yield put(new ReportErrorAction(e.message));
        return;
    }
}