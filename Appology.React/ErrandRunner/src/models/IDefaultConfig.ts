import { IOrderFeeFormula } from "./IOrder";

export interface IDefaultConfig {
    orderFeeFormula: IOrderFeeFormula[],
    serviceFee: number,
    deliveryFeePerMile: number,
    driverFee: number,
    minimumOrderValue: number
}