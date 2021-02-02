import IStoreState from '../../../state/IStoreState';
import { IDateFilter } from '../../../models/IDateFilter';

export const incomeSummaryDateFilter = (state: IStoreState): IDateFilter => {
    return {
        frequency: state.incomeSummary.dateFilter.frequency,
        interval: state.incomeSummary.dateFilter.interval,
        fromDateRange: state.incomeSummary.dateFilter.fromDateRange,
        toDateRange: state.incomeSummary.dateFilter.toDateRange
    };
}
export const getSelectedDateFilter = (state: IStoreState): IDateFilter | null => {
    if (state.common.dateFilter !== undefined) {
        return {
            frequency: state.common.dateFilter.frequency,
            interval: state.common.dateFilter.interval,
            fromDateRange: state.common.dateFilter.fromDateRange,
            toDateRange: state.common.dateFilter.toDateRange
        };
    } else {
        return null
    }
}

export const spendingSummaryDateFilter = (state: IStoreState): IDateFilter => {
    return {
        frequency: state.spendingSummary.dateFilter.frequency,
        interval: state.spendingSummary.dateFilter.interval,
        fromDateRange: state.spendingSummary.dateFilter.fromDateRange,
        toDateRange: state.spendingSummary.dateFilter.toDateRange
    };
}