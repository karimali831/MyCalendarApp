export default interface ICWTLState {
    error: string | null
}

export class CWTLState {
    public static readonly intialState = {
        error: null
    }
}