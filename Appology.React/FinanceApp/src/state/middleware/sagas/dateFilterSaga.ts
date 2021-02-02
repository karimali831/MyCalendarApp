import { put, takeLatest } from "redux-saga/effects";
import { CommonActionTypes, DateFilterChangeAction } from 'src/state/contexts/common/Actions';
import { LoadSpendingSummaryAction, LoadIncomeSummaryAction, SpendingSummaryDateFilterChangeAction, IncomeSummaryDateFilterChangeAction } from 'src/state/contexts/landing/Actions';
import { DataType } from 'src/enums/DataType';

export function* dateFilterWatchSaga() {
    yield takeLatest(CommonActionTypes.DateFilterChange, dateFilterChange)
}

export function* dateFilterChange(action: DateFilterChangeAction) {

    if (action.dataType === DataType.IncomeSummary) {
        yield put(new LoadIncomeSummaryAction(action.filter));
        yield put(new IncomeSummaryDateFilterChangeAction(action.filter))
    } 
    else if (action.dataType === DataType.SpendingSummary ) {
        yield put(new LoadSpendingSummaryAction(action.filter));
        yield put(new SpendingSummaryDateFilterChangeAction(action.filter))
    }
}