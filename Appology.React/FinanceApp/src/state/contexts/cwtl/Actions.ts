
// action types
export class CWTLActionTypes {
    public static readonly CWTL = "@@cwtl/cwtl"
}

// action defined as classes with a creator as a static function

export class CWTLAction {
    public static readonly creator = (CWTLMessage: string) => new CWTLAction(CWTLMessage);

    public readonly type = CWTLActionTypes.CWTL;

    constructor(
        public CWTLMessage: string,
    ) { }
}

// Create a discriminated union of all action typed used to correctly type the
// actions in the reducer switch statement
export type CWTLActions =
    CWTLAction 