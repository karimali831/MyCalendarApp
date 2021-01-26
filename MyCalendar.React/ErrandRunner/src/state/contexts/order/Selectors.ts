import { IDefaultConfig } from "src/models/IDefaultConfig";
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

export const getOrderForm = (state: IStoreState) : IOrderForm => {
    return state.order.orderForm;
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
