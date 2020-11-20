import { ICustomer } from 'src/models/ICustomer';

// action types
export class LandingActionTypes {

    public static readonly LoadCustomers = "@@Landing/loadcustomers";
    public static readonly LoadCustomersSuccess = "@@Landing/loadcustomerssuccess";
    public static readonly LoadCustomersFailure = "@@Landing/loadcustomersfailure";
    public static readonly FilterChanged = "@@landingsummary/filterchanged";
}

export class LoadCustomersAction {
    public static readonly creator = () => new LoadCustomersAction();

    public readonly type = LandingActionTypes.LoadCustomers
}

export class LoadCustomersSuccessAction {
    public static readonly creator = (customers: ICustomer[]) => new LoadCustomersSuccessAction(customers);

    public readonly type = LandingActionTypes.LoadCustomersSuccess;

    constructor(
        public customers: ICustomer[]
    ) { }
}

export class LoadCustomersFailureAction {
    public static readonly creator = (errorMsg: string) => new LoadCustomersFailureAction(errorMsg);

    public readonly type = LandingActionTypes.LoadCustomersFailure;

    constructor(
        public errorMsg: string
    ) { }
}


export class FilterChangedAction {
    public static readonly creator = (filter: string) => new FilterChangedAction(filter);

    public readonly type = LandingActionTypes.FilterChanged;

    constructor(
        public filter: string,
    ) { }
}

// Create a discriminated union of all action types used to correctly type the
// actions in the reducer switch statement
export type LandingActions =
    LoadCustomersAction |
    LoadCustomersSuccessAction |
    LoadCustomersFailureAction |
    FilterChangedAction
