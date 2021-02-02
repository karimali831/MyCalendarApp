import IStoreState from '../../../../../state/IStoreState';
import { connect } from 'react-redux';
import ChartForSummary, { IPropsFromState, IPropsFromDispatch } from '../../../../charts/ChartProps';
import { DateFilterChangeAction } from 'src/state/contexts/common/Actions';
import { ChartType, ChartDataType } from 'src/enums/ChartType';
import { chartSummaryData } from 'src/state/contexts/chart/Selectors';
import { DataType } from 'src/enums/DataType';
import { CategoryType } from 'src/enums/CategoryType';
import { SpendingSummaryMaxCatsChangeAction } from 'src/state/contexts/landing/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        loading: state.chart.loading,
        chart: chartSummaryData(state, DataType.SpendingSummary),
        headerText: "Total: £" + state.spendingSummary.totalSpent,
        dataLabels: true,
        chartType: ChartType.Doughnut,
        width: 200,
        height: 500,
        dateFilter: state.spendingSummary.dateFilter,
        dataType: DataType.SpendingSummary,
        categoryType: CategoryType.Spendings,
        chartDataType: ChartDataType.SpendingSummary,
        maxCats: state.spendingSummary.maxCats
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    dateFilterChanged: DateFilterChangeAction.creator,
    maxCatsChanged: SpendingSummaryMaxCatsChangeAction.creator
};


// This does the magic of subscwribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(ChartForSummary);
