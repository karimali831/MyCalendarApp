import { Variant } from "@appology/react-components";
import { SaveStatus } from "src/Enums/SaveStatus";
import { OrderAction } from "src/models/IDispatchStatus";
import { IOrderForm, IOrderOverview } from "src/models/IOrder";
import { IOrder } from "src/models/IOrder";
import { ITrip } from "src/models/ITrip";

// action types
export class OrderActionTypes {
    public static readonly ToggleConfig = "@@order/toggleconfig";
    public static readonly UpdateOrder = "@@order/updateorder";
    public static readonly OrderOverview = "@@order/orderoverview";
    public static readonly SaveOrder = "@@order/saveorder";
    public static readonly SaveOrderSuccess = "@@order/saveordersuccess";
    public static readonly SaveOrderFailure = "@@order/saveorderfailure";
    public static readonly SelectedOrder = "@@order/selectedorder";
    public static readonly ResetOrder = "@@order/resetorder";
    public static readonly SetDeliveryDate = "@@order/setdeliverydate";
    public static readonly DispatchOrder= "@@order/dispatchorder"
    public static readonly OrderStatus = "@@order/orderstatus"
    public static readonly OrderPaid = "@@order/orderpaid"
    public static readonly SaveStatus = "@@order/savestatus"
}

export class SaveOrderAction {
    public static readonly creator = (saved?: boolean) => new SaveOrderAction(saved);

    public readonly type = OrderActionTypes.SaveOrder

    constructor(
        public saved?: boolean
    ) { }
}

export class SelectedOrderAction {
    public static readonly creator = (order: IOrder | undefined, trip: ITrip | undefined) => new SelectedOrderAction(order, trip);

    public readonly type = OrderActionTypes.SelectedOrder;

    constructor(
        public order: IOrder | undefined,
        public trip: ITrip | undefined
    ) { }
}

export class ResetOrderAction {
    public static readonly creator = () => new ResetOrderAction();

    public readonly type = OrderActionTypes.ResetOrder;
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

export class SetDeliveryDateAction {
    public static readonly creator = (deliveryDate?: Date, timeslot?: string) => new SetDeliveryDateAction(deliveryDate, timeslot);

    public readonly type = OrderActionTypes.SetDeliveryDate;

    constructor(
        public deliveryDate?: Date,
        public timeSlot?: string
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

export class DispatchOrderAction {
    public static readonly creator = (dispatch: boolean) => new DispatchOrderAction(dispatch);

    public readonly type = OrderActionTypes.DispatchOrder;

    constructor(
        public dispatch: boolean
    ) { }
}

export class OrderStatusAction {
    public static readonly creator = (action: OrderAction, variant: Variant, show: boolean) => new OrderStatusAction(action, variant, show);

    public readonly type = OrderActionTypes.OrderStatus;

    constructor(
        public action: OrderAction,
        public variant: Variant,
        public show: boolean
    ) { }
}

export class OrderPaidAction {
    public static readonly creator = (paid: boolean, stripePaymentConfirmationId?: string) => new OrderPaidAction(paid, stripePaymentConfirmationId);

    public readonly type = OrderActionTypes.OrderPaid;

    constructor(
        public paid: boolean,
        public stripePaymentConfirmationId?: string
    ) { }
}

export class SaveStatusAction {
    public static readonly creator = (status: SaveStatus) => new SaveStatusAction(status);

    public readonly type = OrderActionTypes.SaveStatus;

    constructor(
        public status: SaveStatus
    ) { }
}

// Create a discriminated union of all action types used to correctly type the
// actions in the reducer switch statement
export type OrderActions =
    ToggleConfigAction |
    UpdateOrderAction |
    OrderOverviewAction |
    SaveOrderAction |
    SaveOrderSuccessAction |
    SaveOrderFailureAction |
    SelectedOrderAction |
    ResetOrderAction |
    SetDeliveryDateAction |
    DispatchOrderAction |
    OrderStatusAction |
    OrderPaidAction |
    SaveStatusAction 