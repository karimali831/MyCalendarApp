import { PaymentStatus } from 'src/enums/PaymentStatus';

export interface IFinance {
    id: number,
    name: string,
    avgMonthlyAmount: number,
    startDate: Date,
    endDate: Date,
    monthlyDueDate: number,
    nextDueDate: Date,
    overrideNextDueDate: number,
    remaining: number,
    totalAmount: number,
    totalPaid: number,
    manualPayment: boolean,
    daysUntilDue: number,
    paymentStatus: PaymentStatus,
    directDebit: boolean,
    monzoTag: string,
    superCatId: number
}

export interface IFinanceDTO {
    name: string,
    catId: number,
    secondCatId?: number
}