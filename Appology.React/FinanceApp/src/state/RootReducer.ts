import IStoreState from './IStoreState';
import { combineReducers } from 'redux';
import commonReducer from './contexts/common/Reducer';
import notificationReducer from './contexts/landing/INotificationReducer';
import incomeReducer from './contexts/income/Reducer';
import spendingReducer from './contexts/spending/Reducer';
import { connectRouter } from 'connected-react-router';
import { History } from 'history';
import IncomeSummaryReducer from './contexts/landing/IncomeSummaryReducer';
import SpendingSummaryReducer from './contexts/landing/SpendingSummaryReducer';
import chartReducer from './contexts/chart/Reducer'

// Root reducer combining all other state reducers
export default
    (history: History<any>) =>
        combineReducers<IStoreState>({
            router: connectRouter(history),
            notification: notificationReducer,
            spendingSummary: SpendingSummaryReducer,
            incomeSummary: IncomeSummaryReducer,
            common: commonReducer,
            income: incomeReducer,
            spending: spendingReducer,
            chart: chartReducer
        }); 