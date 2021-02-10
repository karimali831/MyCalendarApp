
import { all, fork } from 'redux-saga/effects';
import loadStakeholdersApiSaga from './loadUserApiSaga';
import locationChangeSaga from './locationChangeSaga';

// We `fork()` these tasks so they execute in the background.
export function* rootSaga() {
  yield all([
    // Routing
    fork(locationChangeSaga),

    fork(loadStakeholdersApiSaga),

  ])
}