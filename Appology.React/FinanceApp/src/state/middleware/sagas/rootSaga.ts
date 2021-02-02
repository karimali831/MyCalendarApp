
import { all, fork } from 'redux-saga/effects';
import loadSpendingsApiSaga from './loadSpendingsApiSaga';
import locationChangeSaga from './locationChangeSaga';
import loadSpendingSummaryApiSaga from './loadSpendingSummaryApiSaga';
import { dateFilterWatchSaga } from './dateFilterSaga';
import loadIncomeSummaryApiSaga from './loadIncomeSummaryApiSaga';
import loadIncomesApiSaga from './loadIncomesApiSaga';
import loadNotificationApiSaga from './loadNotificationsApiSaga';
import loadCategoriesApiSaga from './loadCategoriesApiSaga';
import loadChartApiSaga from './loadChartApiSaga';

// We `fork()` these tasks so they execute in the background.
export function* rootSaga() {
  yield all([
    // Routing
    fork(locationChangeSaga),

    // Spendings
    fork(loadSpendingsApiSaga),

    // Income
    fork(loadIncomesApiSaga),

    // Summary
    fork(loadNotificationApiSaga),
    fork(loadSpendingSummaryApiSaga),
    fork(loadIncomeSummaryApiSaga),

    // utils
    fork(dateFilterWatchSaga),
    fork(loadCategoriesApiSaga),

    // charts
    fork(loadChartApiSaga)

  ])
}