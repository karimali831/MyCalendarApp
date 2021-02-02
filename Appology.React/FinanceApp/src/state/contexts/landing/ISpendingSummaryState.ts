import { ISpendingSummary } from "../../../models/ISpending";
import { CategoryType } from 'src/enums/CategoryType';
import { IDateFilter } from 'src/models/IDateFilter';
import { DateFrequency } from 'src/enums/DateFrequency';

export default interface ISpendingSummaryState {
    spendingSummary: ISpendingSummary[],
    spendingSummaryOverview: ISpendingSummary[],
    totalSpent: number,
    loading: boolean,
    showSecondCatSummary: string | null,
    categoryType: CategoryType,
    dateFilter: IDateFilter,
    categoryFilter: string | undefined,
    maxCats: number
}

export const spendingSummaryDateFilter: IDateFilter = {
    frequency: DateFrequency.Today,
    interval: 1,
    fromDateRange: null,
    toDateRange: null
}

export class SpendingSummaryState {

    public static readonly intialState = {
        spendingSummary: [],
        spendingSummaryOverview: [],
        totalSpent: 0,
        loading: false,
        showSecondCatSummary: null,
        categoryType: CategoryType.Spendings,
        dateFilter: spendingSummaryDateFilter,
        categoryFilter: undefined,
        maxCats: 10
    }
}

