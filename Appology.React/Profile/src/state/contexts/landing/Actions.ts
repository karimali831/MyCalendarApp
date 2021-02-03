import { IModel } from "src/models/IModel";

// action types
export class LandingActionTypes {
    public static readonly SelectedAction = "@@landing/selectedaction";
    public static readonly LoadDummySuccessAction = "@@landing/loaddummysuccessaction";
    public static readonly LoadDummyFailureAction = "@@landing/loaddummyfailureaction";
}

export class SelectedAction {
    public static readonly creator = () => new SelectedAction();

    public readonly type = LandingActionTypes.SelectedAction;
}

export class LoadDummySuccessAction {
    public static readonly creator = (result: IModel[]) => new LoadDummySuccessAction(result);

    public readonly type = LandingActionTypes.LoadDummySuccessAction

    constructor(
        public result: IModel[]
    ) { }
}

export class LoadDummyFailureAction {
    public static readonly creator = (errorMsg: string) => new LoadDummyFailureAction(errorMsg);

    public readonly type = LandingActionTypes.LoadDummyFailureAction

    constructor(
        public errorMsg: string
    ) { }
}


// Create a discriminated union of all action types used to correctly type the
// actions in the reducer switch statement
export type LandingActions =
    SelectedAction |
    LoadDummySuccessAction |
    LoadDummyFailureAction