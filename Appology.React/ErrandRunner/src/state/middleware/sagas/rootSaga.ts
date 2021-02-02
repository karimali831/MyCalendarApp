
import { all, fork } from 'redux-saga/effects';
import loadStakeholdersApiSaga from './loadStakeholdersApiSaga';
import locationChangeSaga from './locationChangeSaga';
import orderApiSaga from './orderApiSaga';
import tripApiSaga from './tripApiSaga';

// We `fork()` these tasks so they execute in the background.
export function* rootSaga() {
  yield all([
    // Routing
    fork(locationChangeSaga),

    // New order
    fork(loadStakeholdersApiSaga),
    fork(tripApiSaga),
    fork(orderApiSaga)
  ])
}