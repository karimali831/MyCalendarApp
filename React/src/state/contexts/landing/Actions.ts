import { ICustomer } from 'src/models/ICustomer';

// action types
export class LandingSummaryActionTypes {

    public static readonly LoadCustomers = "@@landingsummary/loadcustomers";
    public static readonly LoadCustomersSuccess = "@@landingsummary/loadcustomerssuccess";
    public static readonly LoadCustomersFailure = "@@landingsummary/loadcustomersfailure";
}

export class LoadCustomersAction {
    public static readonly creator = () => new LoadCustomersAction();

    public readonly type = LandingSummaryActionTypes.LoadCustomers
}

export class LoadCustomersSuccessAction {
    public static readonly creator = (customers: ICustomer[]) => new LoadCustomersSuccessAction(customers);

    public readonly type = LandingSummaryActionTypes.LoadCustomersSuccess;

    constructor(
        public customers: ICustomer[]
    ) { }
}

export class LoadCustomersFailureAction {
    public static readonly creator = (errorMsg: string) => new LoadCustomersFailureAction(errorMsg);

    public readonly type = LandingSummaryActionTypes.LoadCustomersFailure;

    constructor(
        public errorMsg: string
    ) { }
}

// Create a discriminated union of all action types used to correctly type the
// actions in the reducer switch statement
export type LandingSummaryActions =
    LoadCustomersAction |
    LoadCustomersSuccessAction |
    LoadCustomersFailureAction 
