import { SaveStatus } from 'src/Enums/SaveStatus';
import { IDispatchStatus } from 'src/models/IDispatchStatus';
import { IOrder, IOrderForm, IOrderOverview } from 'src/models/IOrder';
import { ITrip } from 'src/models/ITrip';
import { orderStatusDispatch } from './Selectors';

export default interface IOrderState {
    order?: IOrder,
    trip?: ITrip,
    orderForm: IOrderForm,
    orderOverview: IOrderOverview,
    saveOrderStatus: SaveStatus,
    pinSidebar: boolean,
    deliveryDate?: Date,
    timeslot?: string,
    orderStatus: IDispatchStatus[],
    dispatched: boolean,
    paid: boolean,
    stripePaymentConfirmationId?: string
}

export class OrderState {
    public static readonly intialState = {
        order: undefined,
        orderForm: {
            orderId: "",
            items: [{
                name: "",
                qty: 1,
                cost: 0,
                total: 0,
                notes: "",
                maxQuantity: 10
            }],
            orderValue: 0,
            orderFee: 0,
            totalItems: 1,

        },
        orderOverview: {
            tripMiles: 0,
            deliveryFee: 0,
            serviceFee: 0,
            invoiceAmt: 0,
            netProfit: 0,
            driverFee: 0
        },
        saveOrderStatus: SaveStatus.Unprocessing,
        pinSidebar: false,
        deliveryDate: undefined,
        timeslot: undefined,
        orderStatus: orderStatusDispatch,
        dispatched: false,
        paid: false,
        stripePaymentConfirmationId: undefined
    }
}