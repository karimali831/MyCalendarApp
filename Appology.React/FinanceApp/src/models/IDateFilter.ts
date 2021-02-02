import { DateFrequency } from '../enums/DateFrequency'

export interface IDateFilter {
    frequency: DateFrequency,
    interval: number,
    fromDateRange?: string | null,
    toDateRange?: string | null
}