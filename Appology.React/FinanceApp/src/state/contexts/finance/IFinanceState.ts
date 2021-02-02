export default interface IFinanceState {
    error: string | null
}

export class FinanceState {
    public static readonly intialState = {
        error: null
    }
}