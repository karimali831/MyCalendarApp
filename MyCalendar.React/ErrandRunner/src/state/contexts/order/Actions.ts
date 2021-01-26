import { IOrderForm, IOrderOverview } from "src/models/IOrder";
import { IOrder } from "src/models/IOrder";
import { ITrip } from "src/models/ITrip";

// action types
export class OrderActionTypes {
    public static readonly ToggleConfig = "@@order/toggleconfig";
    public static readonly UpdateOrder = "@@order/updateorder";
    public static readonly OrderOverview = "@@order/orderoverview";
    public static readonly SetDriverStep4 = "@@order/setdriverstep4";
    public static readonly SaveOrder = "@@order/saveorder";
    public static readonly SaveOrderSuccess = "@@order/saveordersuccess";
    public static readonly SaveOrderFailure = "@@order/saveorderfailure";
    public static readonly SelectedOrder = "@@order/selectedorder";
}

export class SetDriverStep4Action {
    public static readonly creator = () => new SetDriverStep4Action();

    public readonly type = OrderActionTypes.SetDriverStep4
}

export class SaveOrderAction {
    public static readonly creator = () => new SaveOrderAction();

    public readonly type = OrderActionTypes.SaveOrder
}

export class SelectedOrderAction {
    public static readonly creator = (order: IOrder | undefined, trip: ITrip | undefined) => new SelectedOrderAction(order, trip);

    public readonly type = OrderActionTypes.SelectedOrder;

    constructor(
        public order: IOrder | undefined,
        public trip: ITrip | undefined
    ) { }
}

// this action is called from OrderDetails when any order inputs are altered
export class SaveOrderSuccessAction {
    public static readonly creator = (order: IOrder, trip: ITrip) => new SaveOrderSuccessAction(order, trip);

    public readonly type = OrderActionTypes.SaveOrderSuccess;

    constructor(
        public order: IOrder,
        public trip: ITrip
    ) { }
}

export class SaveOrderFailureAction {
    public static readonly creator = (errorMsg: string) => new SaveOrderFailureAction(errorMsg);

    public readonly type = OrderActionTypes.SaveOrderFailure

    constructor(
        public errorMsg: string
    ) { }
}

export class ToggleConfigAction {
    public static readonly creator = () => new ToggleConfigAction();

    public readonly type = OrderActionTypes.ToggleConfig
}

// this action is called from OrderDetails when any order inputs are altered
export class UpdateOrderAction {
    public static readonly creator = (order: IOrderForm) => new UpdateOrderAction(order);

    public readonly type = OrderActionTypes.UpdateOrder;

    constructor(
        public order: IOrderForm
    ) { }
}


// this action is called from orderSaga it's sets all calculated fees etc.
export class OrderOverviewAction {
    public static readonly creator = (order: IOrderOverview) => new OrderOverviewAction(order);

    public readonly type = OrderActionTypes.OrderOverview;

    constructor(
        public order: IOrderOverview
    ) { }
}

// Create a discriminated union of all action types used to correctly type the
// actions in the reducer switch statement
export type OrderActions =
    ToggleConfigAction |
    UpdateOrderAction |
    OrderOverviewAction |
    SetDriverStep4Action | 
    SaveOrderAction |
    SaveOrderSuccessAction |
    SaveOrderFailureAction |
    SelectedOrderAction
