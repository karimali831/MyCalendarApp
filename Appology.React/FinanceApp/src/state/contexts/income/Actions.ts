import { IIncome } from '../../../models/IIncome';

// action types
export class IncomeActionTypes {
    public static readonly ResetIncomes = "@@incomes/resetincomes";
    public static readonly LoadIncomes = "@@incomes/loadincomes";
    public static readonly LoadIncomesSuccess = "@@incomes/loadincomessuccess";
    public static readonly LoadIncomesFailure = "@@incomes/loadincomesfailure";
}

// actions defined as classes with a creator as a static function
export class ResetIncomesAction {
    public static readonly creator = () => new ResetIncomesAction();

    public readonly type = IncomeActionTypes.ResetIncomes;
}

// actions defined as classes with a creator as a static function
export class LoadIncomesAction {
    public static readonly creator = () => new LoadIncomesAction();

    public readonly type = IncomeActionTypes.LoadIncomes;
}

// actions defined as classes with a creator as a static function
export class LoadIncomesSuccessAction {
    public static readonly creator = (Incomes: IIncome[]) => new LoadIncomesSuccessAction(Incomes);

    public readonly type = IncomeActionTypes.LoadIncomesSuccess;

    constructor(
        public incomes: IIncome[]
    ) { }
}

export class LoadIncomesFailureAction {
    public static readonly creator = (errorMsg: string) => new LoadIncomesFailureAction(errorMsg);

    public readonly type = IncomeActionTypes.LoadIncomesFailure;

    constructor(
        public errorMsg: string
    ) { }
}


// Create a discriminated union of all action types used to correctly type the
// actions in the reducer switch statement
export type IncomeActions =
    ResetIncomesAction |
    LoadIncomesAction |
    LoadIncomesSuccessAction |
    LoadIncomesFailureAction