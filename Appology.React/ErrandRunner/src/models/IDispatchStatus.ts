import { Variant } from "@appology/react-components";

export interface IDispatchStatus {
    stepNo: number,
    orderAction: OrderAction
    pendingTxt: string,
    completeTxt: string,
    errorTxt: string,
    variant?: Variant,
    show: boolean
}

export enum OrderAction {
    DeliveryDate,
    Save,
    PaymentIntent,
    Payment,
    Dispatch
}