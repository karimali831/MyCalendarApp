
// action types
export class ErrorActionTypes {
    public static readonly ReportError = "@@error/reporterror"
    public static readonly HideError = "@@error/hideerror"
}

// action defined as classes with a creator as a static function

export class ReportErrorAction {
    public static readonly creator = (errorMessage: string) => new ReportErrorAction(errorMessage);

    public readonly type = ErrorActionTypes.ReportError;

    constructor(
        public errorMessage: string,
    ) { }
}

export class HideErrorAction {
    public static readonly creator = () => new HideErrorAction();

    public readonly type = ErrorActionTypes.HideError;
}

// Create a discriminated union of all action typed used to correctly type the
// actions in the reducer switch statement
export type ErrorActions =
    ReportErrorAction |
    HideErrorAction