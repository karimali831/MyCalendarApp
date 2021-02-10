import { api, IUserResponse} from '../../../Api/Api'
import { call, put, takeLatest } from 'redux-saga/effects';
import { ReportErrorAction } from '../../contexts/error/Actions';
import { ProfileActionTypes, LoadUserSuccessAction, LoadUserFailureAction } from '../../contexts/profile/Action';

export default function* loadUserApiSaga() {
    yield takeLatest(ProfileActionTypes.LoadUser, loadUser);
}

// export function* loadUserRequest() {
//     const filter: string = yield select(getFilter);

//     yield call(loadUser, filter);
// }

export function* loadUser() {
    try {

        // Start the API call asynchronously
        const result: IUserResponse = yield call(api.user);

        // Create an action to dispatch on success with the returned entity from API
        const resultAction = new LoadUserSuccessAction(result.groups, result.user);

        // Dispatch the new action with Redux
        yield put(resultAction);
        
    } catch (e) {

        // Dispatch a failure action to Redux
        yield put(new LoadUserFailureAction(e.message));
        yield put(new ReportErrorAction(e.message));
        return;
    }
}