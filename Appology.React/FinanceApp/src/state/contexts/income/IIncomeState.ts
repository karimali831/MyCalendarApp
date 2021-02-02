import { IIncome } from "../../../models/IIncome";
import { IDateFilter } from 'src/models/IDateFilter';

export default interface IIncomeState {
    incomes: IIncome[],
    loading: boolean,
    sourceId: number | null,
    dateFilter?: IDateFilter
}

export class IncomeState {
    public static readonly intialState = {
        loading: true,
        incomes: [],
        sourceId: null,
        dateFilter: undefined
    }
}
