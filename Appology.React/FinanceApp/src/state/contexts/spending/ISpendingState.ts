import { ISpending } from "../../../models/ISpending";
import { IDateFilter } from 'src/models/IDateFilter';

export default interface ISpendingState {
    spendings: ISpending[],
    loading: boolean,
    catId?: number | null,
    dateFilter?: IDateFilter,
    isFinance: boolean,
    isSecondCat: boolean
}

export class SpendingState {
    public static readonly intialState = {
        loading: true,
        spendings: [],
        catId: null,
        dateFilter: undefined,
        isFinance: false,
        isSecondCat: false
    }
}
