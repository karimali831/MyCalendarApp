import * as React from 'react';
import './App.css';
import { Route, Switch } from 'react-router-dom';
import { Provider } from 'react-redux';
import initialiseStore from '../../state/InitialiseStore';
import AddSpending from '../roots/spending/AddSpendingConnected';
import AddExpense from '../roots/finance/AddExpenseConnected';
import Incomes from '../roots/income/IncomeConnected'
import AddIncome from '../roots/income/AddIncomeConnected';
import AddCategory from '../roots/category/AddCategory';
import Finances from '../roots/finance/Finances';
import Categories from '../roots/category/Categories';
import { ConnectedRouter } from 'connected-react-router';
import { createBrowserHistory } from 'history';
import Landing from '../roots/landing/Landing';
import Spendings from '../roots/spending/SpendingConnected'
import { CategoryType } from 'src/enums/CategoryType';
import AddReminder from '../roots/reminder/AddReminderConnected';
import Reminders from '../roots/reminder/Reminders';
import ChartForSpendingSummary from '../roots/landing/spendingSummary/connected/ChartForSpendingSummaryConnected';
import ChartForIncomeSummary from '../roots/landing/incomeSummary/connected/ChartForIncomeSummaryConnected';
import ChartForIncomeExpenseSummary from '../roots/landing/ChartForIncomeExpenseSummaryConnected';
import ChartForSpendingSummaryByCat from '../roots/landing/spendingSummary/connected/ChartForSpendingSummaryByCatConnected';
import ChartForIncomeSummaryByCat from '../roots/landing/incomeSummary/connected/ChartForIncomeSummaryByCatConnected';
import ChartForFinancesSummaryConnected from '../roots/landing/ChartForFinancesSummaryConnected';
import { appPathUrl } from '../utils/Utils';
import MenuConnected  from '../base/MenuConnected'

class App extends React.Component {
  private appElement: React.RefObject<HTMLDivElement> = React.createRef<HTMLDivElement>();

  public render() {
    const store = initialiseStore(createBrowserHistory());
    return (
      <Provider store={store}>
        <ConnectedRouter history={(createBrowserHistory())}>
            <MenuConnected />
            <div className="App" ref={this.appElement}>
              <Switch>
                  <Route exact={true} path="/" component={Landing} />
                  <Route exact={true} path={appPathUrl} component={Landing} />
                  <Route path={`${appPathUrl}/home`} component={Landing} />
                  <Route path={`${appPathUrl}/addspending`} component={AddSpending} />
                  <Route path={`${appPathUrl}/addexpense`} component={AddExpense} />
                  <Route path={`${appPathUrl}/addincome`} component={AddIncome} />
                  <Route path={`${appPathUrl}/addcategory`} component={AddCategory} />
                  <Route path={`${appPathUrl}/addreminder`} component={AddReminder} />
                  <Route path={`${appPathUrl}/finance`} component={Finances} />
                  <Route path={`${appPathUrl}/spending`} component={Spendings} />
                  <Route path={`${appPathUrl}/income`} component={Incomes} />
                  <Route path={`${appPathUrl}/${CategoryType[CategoryType.Incomes]}/:catId?/:frequency?/:interval?/:isSecondCat?/:fromDate?/:toDate?`} render={() => <Incomes />} />
                  <Route path={`${appPathUrl}/${CategoryType[CategoryType.Spendings]}/:catId?/:frequency?/:interval?/:isFinance?/:isSecondCat?/:fromDate?/:toDate?`} render={() => <Spendings />} />
                  <Route path={`${appPathUrl}/category`} component={Categories} />
                  <Route path={`${appPathUrl}/reminder`} component={Reminders} />
                  <Route path={`${appPathUrl}/chart/spendingsummary`} component={ChartForSpendingSummary} />
                  <Route path={`${appPathUrl}/chart/incomesummary`} component={ChartForIncomeSummary} />
                  <Route path={`${appPathUrl}/chart/incomeexpense`} component={ChartForIncomeExpenseSummary} />
                  <Route path={`${appPathUrl}/chart/${CategoryType[CategoryType.Spendings]}/summary/:catId/:isSecondCat`} render={() => <ChartForSpendingSummaryByCat />} />
                  <Route path={`${appPathUrl}/chart/${CategoryType[CategoryType.Incomes]}/summary/:catId/:isSecondCat`} render={() => <ChartForIncomeSummaryByCat />} />
                  <Route path={`${appPathUrl}/chart/finances`} component={ChartForFinancesSummaryConnected} />
              </Switch>
            </div>
        </ConnectedRouter>
      </Provider>
    );
  }
}
  
export default App;