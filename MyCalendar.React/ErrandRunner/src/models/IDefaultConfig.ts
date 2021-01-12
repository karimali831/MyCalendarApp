import { IOrderFeeFormula } from "./ServiceCostFormula";

export interface IDefaultConfig {
    orderFeeFormula: IOrderFeeFormula[],
    serviceFee: number,
    deliveryFeePerMile: number,
    driverFee: number
}