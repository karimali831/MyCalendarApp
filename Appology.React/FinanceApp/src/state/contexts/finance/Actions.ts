
// action types
export class FinanceActionTypes {
    public static readonly Finance = "@@finance/finance"
}

// action defined as classes with a creator as a static function

export class FinanceAction {
    public static readonly creator = (FinanceMessage: string) => new FinanceAction(FinanceMessage);

    public readonly type = FinanceActionTypes.Finance;

    constructor(
        public FinanceMessage: string,
    ) { }
}

// Create a discriminated union of all action typed used to correctly type the
// actions in the reducer switch statement
export type FinanceActions =
    FinanceAction 