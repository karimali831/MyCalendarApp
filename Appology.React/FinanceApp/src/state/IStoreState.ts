import ICommonState, { CommonState } from './contexts/common/ICommonState';
import IIncomeState, { IncomeState } from './contexts/income/IIncomeState';
import ISpendingState, { SpendingState } from './contexts/spending/ISpendingState';
import ISpendingSummaryState, { SpendingSummaryState } from './contexts/landing/ISpendingSummaryState';
import { IRouteState, RouteState } from './contexts/router/IRouteState';
import IIncomeSummaryState, { IncomeSummaryState } from './contexts/landing/IIncomeSummaryState';
import INotificationState, { NotificationState } from './contexts/landing/INotificationState';
import IChartState, { ChartState } from './contexts/chart/ChartState';

// this represents the state of the 'entire' application
// it should be composed of other state definitions which represent state 'contexts'
// managed by seperate reducers and have actions which alter those contexts

export default interface IStoreState {
    router: IRouteState,
    common: ICommonState,
    notification: INotificationState,
    income: IIncomeState,
    spending: ISpendingState,
    spendingSummary: ISpendingSummaryState,
    incomeSummary: IIncomeSummaryState,
    chart: IChartState
}

export class StoreState {
    public static readonly initialState: IStoreState = {
        router: RouteState.initialState,
        common: CommonState.intialState,
        notification: NotificationState.intialState,
        income: IncomeState.intialState,
        spending: SpendingState.intialState,
        spendingSummary: SpendingSummaryState.intialState,
        incomeSummary: IncomeSummaryState.intialState,
        chart: ChartState.intialState
    };
}

