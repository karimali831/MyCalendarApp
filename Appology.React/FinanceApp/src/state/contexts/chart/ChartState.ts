
import { IMonthComparisonChartRequest, IMonthComparisonChartResponse } from 'src/Api/Api';
import { ChartDataType } from 'src/enums/ChartType';

export default interface IChartState {
    loading: boolean,
    request?: IMonthComparisonChartRequest | undefined,
    type?: ChartDataType | undefined,
    incomeExpenseComparisonChart?: IMonthComparisonChartResponse | undefined,
    incomeCategoryComparisonChart?: IMonthComparisonChartResponse | undefined,
    expenseCategoryComparisonChart?: IMonthComparisonChartResponse | undefined,
    financesComparisonChart?: IMonthComparisonChartResponse | undefined
}

export class ChartState {
    public static readonly intialState = {
        loading: true,
        request: undefined,
        ChartDataType: undefined,
        incomeExpenseComparisonChart: undefined,
        incomeCategoryComparisonChart: undefined,
        expenseCategoryComparisonChart: undefined,
        financesComparisonChart: undefined
    }
}