import IStoreState from '../../../../../state/IStoreState';
import { connect } from 'react-redux';
import Chart, { IPropsFromState, IPropsFromDispatch } from '../../../../charts/ChartProps';
import { ChartType, ChartDataType } from 'src/enums/ChartType';
import { chartSummaryData } from 'src/state/contexts/chart/Selectors';
import { DateFilterChangeAction } from 'src/state/contexts/common/Actions';
import { DataType } from 'src/enums/DataType';
import { CategoryType } from 'src/enums/CategoryType';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        loading: state.chart.loading,
        chart: chartSummaryData(state, DataType.IncomeSummary),
        headerText: "Total: £" + state.incomeSummary.totalIncome,
        dataLabels: true,
        chartType: ChartType.Doughnut,
        width: 200,
        height: 500,
        dateFilter: state.incomeSummary.dateFilter,
        dataType: DataType.IncomeSummary,
        secondTypeId: state.common.secondTypeId,
        selectedCat: state.common.selectedCat,
        selectedSecondCat: state.common.selectedSecondCat,
        categoryType: CategoryType.Incomes,
        chartDataType: ChartDataType.IncomeSummary
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    dateFilterChanged: DateFilterChangeAction.creator
};


// This does the magic of subscwribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(Chart);
