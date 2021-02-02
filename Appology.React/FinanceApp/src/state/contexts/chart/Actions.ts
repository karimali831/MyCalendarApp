import { IMonthComparisonChartRequest, IMonthComparisonChartResponse } from 'src/Api/Api';
import { ChartDataType } from 'src/enums/ChartType';

// action types
export class ChartActionTypes {
    public static readonly LoadChart = "@@chart/loadchart"
    public static readonly LoadIncomeExpenseChartSuccess = "@@chart/loadincomeexpensechartsuccess"
    public static readonly LoadIncomesByCategoryChartSuccess = "@@chart/loadincomesbycategorychartsuccess"
    public static readonly LoadExpensesByCategoryChartSuccess = "@@chart/loadexpensesbycategorychartsuccess"
    public static readonly LoadFinancesChartSuccess = "@@chart/loadfinanceschartsuccess"
    public static readonly LoadChartFailure = "@@chart/loadchartfailure"
}

// action defined as classes with a creator as a static function
export class LoadChartAction {
    public static readonly creator = (request: IMonthComparisonChartRequest, chartDataType: ChartDataType) => new LoadChartAction(request, chartDataType);

    public readonly type = ChartActionTypes.LoadChart;

    constructor(
        public request: IMonthComparisonChartRequest,
        public chartDataType: ChartDataType
    ) { }
}

export class LoadIncomeExpenseChartSuccessAction {
    public static readonly creator = (chart: IMonthComparisonChartResponse) => new LoadIncomeExpenseChartSuccessAction(chart);

    public readonly type = ChartActionTypes.LoadIncomeExpenseChartSuccess;

    constructor(
        public chart: IMonthComparisonChartResponse
    ) { }
}

export class LoadIncomesByCategoryChartSuccessAction {
    public static readonly creator = (chart: IMonthComparisonChartResponse) => new LoadIncomesByCategoryChartSuccessAction(chart);

    public readonly type = ChartActionTypes.LoadIncomesByCategoryChartSuccess;

    constructor(
        public chart: IMonthComparisonChartResponse
    ) { }
}

export class LoadExpensesByCategoryChartSuccessAction {
    public static readonly creator = (chart: IMonthComparisonChartResponse) => new LoadExpensesByCategoryChartSuccessAction(chart);

    public readonly type = ChartActionTypes.LoadExpensesByCategoryChartSuccess;

    constructor(
        public chart: IMonthComparisonChartResponse
    ) { }
}

export class LoadFinancesChartSuccessAction {
    public static readonly creator = (chart: IMonthComparisonChartResponse) => new LoadFinancesChartSuccessAction(chart);

    public readonly type = ChartActionTypes.LoadFinancesChartSuccess;

    constructor(
        public chart: IMonthComparisonChartResponse
    ) { }
}

export class LoadChartFailureAction {
    public static readonly creator = (errorMsg: string) => new LoadChartFailureAction(errorMsg);

    public readonly type = ChartActionTypes.LoadChartFailure;

    constructor(
        public errorMsg: string
    ) { }
}


// Create a discriminated union of all action typed used to correctly type the
// actions in the reducer switch statement
export type ChartActions =
LoadChartAction |
LoadIncomeExpenseChartSuccessAction |
LoadIncomesByCategoryChartSuccessAction |
LoadExpensesByCategoryChartSuccessAction |
LoadFinancesChartSuccessAction |
LoadChartAction