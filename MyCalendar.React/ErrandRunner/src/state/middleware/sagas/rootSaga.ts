
import { all, fork } from 'redux-saga/effects';
import locationChangeSaga from './locationChangeSaga';
// import loadCustomersApiSaga from './loadCustomersApiSaga';

// We `fork()` these tasks so they execute in the background.
export function* rootSaga() {
  yield all([
    // Routing
    fork(locationChangeSaga),

    // // Summary
    // fork(loadCustomersApiSaga)
  ])
}