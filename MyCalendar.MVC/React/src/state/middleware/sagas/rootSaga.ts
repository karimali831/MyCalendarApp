
import { all, fork } from 'redux-saga/effects';
import locationChangeSaga from './locationChangeSaga';
import loadNotificationApiSaga from './LoadCustomersApiSaga';

// We `fork()` these tasks so they execute in the background.
export function* rootSaga() {
  yield all([
    // Routing
    fork(locationChangeSaga),

    // Summary
    fork(loadNotificationApiSaga)
  ])
}