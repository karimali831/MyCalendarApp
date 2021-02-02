import { PaymentStatus } from 'src/enums/PaymentStatus';

export interface IReminder {
    id: number,
    notes: string,
    dueDate?: Date
    daysUntilDue?: number,
    paymentStatus: PaymentStatus
    priority: string,
    category: string,
    sort: number
}

export interface IReminderDTO {
    notes: string,
    dueDate?: string | null,
    priority: number,
    catId: number
}

export interface IReminderNotification {
    overDueReminders: IReminder[],
    upcomingReminders: IReminder[],
    dueTodayReminders: IReminder[],
    alerts: IReminder[],
    summary: ISummary
}

export interface ISummary {
    estimatedBalance: string,
    remainingCash: string,
    accruedSavings: string
}