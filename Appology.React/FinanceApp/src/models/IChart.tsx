import { ChartDataSets } from 'chart.js';
import { Moment } from 'moment';

import { CategoryType } from 'src/enums/CategoryType';

export interface IMonthComparisonChart {
    yearMonth: string,
    monthName: string,
    total: number,
    type: CategoryType,
    category: string,
    secondCategory: string
}

export interface IChartSummary{
    title: string,
    averagedDaily: string,
    averagedMonthly: string,
    totalSpent: string
}

export interface IChartModel {
    labels?: Array<string | string[] | number | number[] | Date | Date[] | Moment | Moment[]>;
    datasets?: ChartDataSets[];
}