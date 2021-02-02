import { ISpending } from '../../../models/ISpending';

// action types
export class SpendingActionTypes {
    public static readonly ResetSpendings = "@@spendings/resetspendings";
    public static readonly LoadSpendings = "@@spendings/loadspendings";
    public static readonly LoadSpendingsSuccess = "@@spendings/loadspendingssuccess";
    public static readonly LoadSpendingsFailure = "@@spendings/loadspendingsfailure";
}

// actions defined as classes with a creator as a static function
export class ResetSpendingsAction {
    public static readonly creator = () => new ResetSpendingsAction();

    public readonly type = SpendingActionTypes.ResetSpendings;
}

// actions defined as classes with a creator as a static function
export class LoadSpendingsAction {
    public static readonly creator = () => new LoadSpendingsAction();

    public readonly type = SpendingActionTypes.LoadSpendings;
}

// actions defined as classes with a creator as a static function
export class LoadSpendingsSuccessAction {
    public static readonly creator = (spendings: ISpending[]) => new LoadSpendingsSuccessAction(spendings);

    public readonly type = SpendingActionTypes.LoadSpendingsSuccess;

    constructor(
        public spendings: ISpending[]
    ) { }
}

export class LoadSpendingsFailureAction {
    public static readonly creator = (errorMsg: string) => new LoadSpendingsFailureAction(errorMsg);

    public readonly type = SpendingActionTypes.LoadSpendingsFailure;

    constructor(
        public errorMsg: string
    ) { }
}


// Create a discriminated union of all action types used to correctly type the
// actions in the reducer switch statement
export type SpendingActions =
    ResetSpendingsAction |
    LoadSpendingsAction |
    LoadSpendingsSuccessAction |
    LoadSpendingsFailureAction