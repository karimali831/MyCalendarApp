export default interface IErrorState {
    error: string | null
}

export class ErrorState {
    public static readonly intialState = {
        error: null
    }
}