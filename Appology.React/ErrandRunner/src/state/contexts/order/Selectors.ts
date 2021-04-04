import { Variant } from "@appology/react-components";
import { SaveStatus } from "src/Enums/SaveStatus";
import { IDefaultConfig } from "src/models/IDefaultConfig";
import { IDispatchStatus, OrderAction } from "src/models/IDispatchStatus";
import { IOrder, IOrderForm, IOrderOverview } from "src/models/IOrder";
import { ITrip } from "src/models/ITrip";
import IStoreState from "src/state/IStoreState";
import ILandingState from "../landing/ILandingState";

export const getOrderOverview = (state: IStoreState): IOrderOverview => {
    return {
        tripMiles: tripMiles(state.landing),
        deliveryFee: orderDeliveryFee(state.landing),
        serviceFee: orderServiceFee(state),
        invoiceAmt: orderInvoiceAmt(state),
        netProfit: netProfit(state),
        driverFee: netProfit(state) * config(state.landing).driverFee
    };
}

export const saveOrderStatus = (state: IStoreState) : SaveStatus => {
    return state.order.saveOrderStatus
}

export const getDeliveryDate = (state: IStoreState) : Date | undefined => {
    return state.order.deliveryDate
}

export const getDeliveryTimeslot = (state: IStoreState) : string | undefined => {
    return state.order.timeslot
}

export const getOrderPaid = (state: IStoreState) : boolean => {
    return state.order.paid
}

export const getStripePaymentConfirmationId = (state: IStoreState) : string | undefined => {
    return state.order.stripePaymentConfirmationId
}

export const getOrderDispatch = (state: IStoreState) : boolean => {
    return state.order.dispatched
}

export const getOrderForm = (state: IStoreState) : IOrderForm => {
    return state.order.orderForm
}

export const getOrder = (state: IStoreState) : IOrder | undefined => {
    return state.order.order
}

export const getTrip = (state: IStoreState) : ITrip | undefined => {
    return state.order.trip
}

export const netProfit = (state: IStoreState) : number => {
    return orderInvoiceAmt(state) - getOrderForm(state).orderValue;
}

export const orderInvoiceAmt = (state: IStoreState) : number => {
    return getOrderForm(state).orderValue + getOrderForm(state).orderFee + orderServiceFee(state) + orderDeliveryFee(state.landing);
}

export const config = (state: ILandingState) : IDefaultConfig => {
    return state.config;
}

export const orderDeliveryFee = (state: ILandingState) : number => {
    return tripMiles(state) * config(state).deliveryFeePerMile;
}

export const orderServiceFee = (state: IStoreState) : number => {
    return config(state.landing).serviceFee * getOrderForm(state).orderValue;
}

export const tripMiles = (state: ILandingState): number => {
    return Number(state.tripOverview?.distance.replace(/\D+$/g, ""));
}

export const orderStatusDispatch : IDispatchStatus[] = [
    {
        stepNo: 5,
        pendingTxt: "Delivery date/timeslot not set", 
        completeTxt: "Delivery date set",
        errorTxt: "Error allocating delivery date/timeslot",
        orderAction: OrderAction.DeliveryDate,
        variant: Variant.Warning,
        show: true
    }, 
    {
        stepNo: 5,
        pendingTxt: "Error creating payment intent", 
        errorTxt: "Error creating payment intent",
        completeTxt: "Payment ready",
        orderAction: OrderAction.PaymentIntent,
        show: false
    },
    {
        stepNo: 5,
        pendingTxt: "Payment not received", 
        errorTxt: "Payment failed",
        completeTxt: "Payment received",
        orderAction: OrderAction.Payment,
        variant: Variant.Warning,
        show: true
    },
    {
        stepNo: 5,
        pendingTxt: "Order not saved",
        errorTxt: "Error saving order",
        completeTxt: "Order saved",
        orderAction: OrderAction.Save,
        show: false
    },
    {
        stepNo: 5,
        pendingTxt: "Order not dispatched",
        errorTxt: "Error dispatching",
        completeTxt: "Order dispatched",
        orderAction: OrderAction.Dispatch,
        show: false
    }
]