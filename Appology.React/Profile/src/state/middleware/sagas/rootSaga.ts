
import { all, fork } from 'redux-saga/effects';
import loadStakeholdersApiSaga from './loadDummyApiSaga';
import locationChangeSaga from './locationChangeSaga';

// We `fork()` these tasks so they execute in the background.
export function* rootSaga() {
  yield all([
    // Routing
    fork(locationChangeSaga),

    fork(loadStakeholdersApiSaga),

  ])
}