import * as React from 'react';
import { faArrowUp, faChartPie, faChartLine } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import DateFilter from './connected/DateFilterISConnected'
import { IIncomeSummary } from '../../../../models/IIncome';
import { Load } from '../../../base/Loader';
import { IDateFilter } from 'src/models/IDateFilter';
import { ShowSecondCategoryIncomeSummaryAction } from 'src/state/contexts/landing/Actions';
import { CategoryType } from 'src/enums/CategoryType';
import SummaryList from '../SummaryList';
import { Link } from 'react-router-dom';
import { IMonthComparisonChartRequest } from 'src/Api/Api';
import { LoadChartAction } from 'src/state/contexts/chart/Actions';
import { ChartDataType } from 'src/enums/ChartType';
import { OnChangeSelectedCategoryAction, OnChangeSelectedSecondCategoryAction } from 'src/state/contexts/common/Actions';
import { appPathUrl } from 'src/components/utils/Utils';

export interface IPropsFromState {
    dateFilter: IDateFilter,
    incomeSummary: IIncomeSummary[],
    totalIncome: number,
    loading: boolean,
    showSecondCatSummary: string | null,
    categoryType: CategoryType,
    location: string
}

export interface IPropsFromDispatch {
    showSecondCategory: (secondCat: string) => ShowSecondCategoryIncomeSummaryAction
    loadIncomeExpense: (request: IMonthComparisonChartRequest, chartDataType: ChartDataType) => LoadChartAction,
    loadIncomesByCategory: (request: IMonthComparisonChartRequest, chartDataType: ChartDataType) =>  LoadChartAction
    onChangeSelectedCategory: (selectedCat: number, secondTypeId?: number) => OnChangeSelectedCategoryAction,
    onChangeSelectedSecondCategory: (selectedSecondCat: number) => OnChangeSelectedSecondCategoryAction
}

type AllProps = IPropsFromState & IPropsFromDispatch;

export default class IncomeSummary extends React.Component<AllProps> {

    public render() {
        if (this.props.loading) {
            return <Load text="Loading income summary..." />
        }

        const incomeExpenseChartRequest: IMonthComparisonChartRequest = {
            dateFilter: this.props.dateFilter
        }

        return (
            <table className="table">
                <thead className="thead-light">
                    <tr>
                        <th scope="col" colSpan={2}>
                            <Link to={`${appPathUrl}/chart/incomeexpense`} onClick={() => this.props.loadIncomeExpense(incomeExpenseChartRequest, ChartDataType.IncomeExpenseSummary)}>
                                <FontAwesomeIcon icon={faChartLine} /> Income and expenses summary chart
                            </Link>
                            <br />
                            <Link to={`${appPathUrl}/chart/incomesummary`}> 
                                <FontAwesomeIcon icon={faChartPie} /> Income breakdown summary for
                            </Link>   
                            <DateFilter />
                        </th>
                    </tr>
                </thead>
                <tbody>
                    <SummaryList<IIncomeSummary>
                        showSecondCategory={this.props.showSecondCategory}
                        showSecondCatSummary={this.props.showSecondCatSummary}
                        showChartByCategory={this.props.loadIncomesByCategory}
                        onChangeSelectedCategory={this.props.onChangeSelectedCategory}
                        onChangeSelectedSecondCategory={this.props.onChangeSelectedSecondCategory}
                        categoryType={this.props.categoryType}
                        filteredResults={this.props.incomeSummary}
                        dateFilter={this.props.dateFilter}
                        chartDataType={ChartDataType.IncomeSummaryByCategory}
                        icon={faArrowUp}
                    />
                    <tr>
                        <th scope="row"><FontAwesomeIcon icon={faArrowUp} /> Total Income</th>
                        <td>£{this.props.totalIncome}</td>
                    </tr>
                </tbody>
            </table>
        )
    }
}