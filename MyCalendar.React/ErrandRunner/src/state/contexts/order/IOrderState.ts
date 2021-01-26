import { IOrder, IOrderForm, IOrderOverview } from 'src/models/IOrder';
import { ITrip } from 'src/models/ITrip';

export default interface IOrderState {
    order?: IOrder,
    trip?: ITrip,
    orderForm: IOrderForm,
    orderOverview: IOrderOverview,
    saveOrderLoading: boolean,
    saveOrderStatus?: boolean,
    pinSidebar: boolean
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
                notes: ""
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
        saveOrderLoading: false,
        saveOrderStatus: undefined,
        pinSidebar: false
    }
}