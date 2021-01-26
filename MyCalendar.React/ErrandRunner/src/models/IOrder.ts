export interface IOrderForm {
    orderId: string,
    items: IOrderItem[],
    orderValue: number,
    orderFee: number,
    totalItems: number
}

export interface IOrderItem {
    name: string,
    qty: number,
    cost: number,
    notes: string
}

export interface IOrderFeeFormula {
    orderValueMin: number,
    orderValueMax: number,
    fee: number
}

export interface IOrderOverview {
    tripMiles: number,
    deliveryFee: number,
    serviceFee: number,
    invoiceAmt: number,
    netProfit: number,
    driverFee: number;
}

export interface IOrder {
    orderId: string,
    customerId: string,
    serviceId: number,
    serviceName?: string,
    items: string,
    orderValue: number,
    serviceFee: number,
    orderFee: number,
    deliveryFee: number,
    totalItems: number,
    invoice: number,
    net: number,
    driverFee: number,
    driverEarning: number
}