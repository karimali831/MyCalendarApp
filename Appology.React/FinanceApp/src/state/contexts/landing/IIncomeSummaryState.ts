import { IIncomeSummary } from 'src/models/IIncome';
import { CategoryType } from 'src/enums/CategoryType';
import { IDateFilter } from 'src/models/IDateFilter';
import { DateFrequency } from 'src/enums/DateFrequency';
import { monthNames } from 'src/components/utils/Utils';

export default interface IIncomeSummaryState {
    incomeSummary: IIncomeSummary[],
    totalIncome: number,
    loading: boolean,
    showSecondCatSummary: string | null,
    categoryType: CategoryType,
    dateFilter: IDateFilter
}

export const incomeSummaryDateFilter: IDateFilter = {
    frequency: DateFrequency[monthNames[new Date().getMonth()]],
    interval: 1,
    fromDateRange: null,
    toDateRange: null
}

export class IncomeSummaryState {
    public static readonly intialState = {
        incomeSummary: [],
        totalIncome: 0,
        loading: false,
        showSecondCatSummary: null,
        categoryType: CategoryType.Incomes,
        dateFilter: incomeSummaryDateFilter
    }
}