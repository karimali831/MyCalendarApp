import IStoreState from '../../../../../state/IStoreState';
import { connect } from 'react-redux';
import IncomeSummary, { IPropsFromState, IPropsFromDispatch } from '../IncomeSummary';
import { ShowSecondCategoryIncomeSummaryAction } from '../../../../../state/contexts/landing/Actions';
import { LoadChartAction } from 'src/state/contexts/chart/Actions';
import { OnChangeSelectedCategoryAction, OnChangeSelectedSecondCategoryAction } from 'src/state/contexts/common/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        dateFilter: state.incomeSummary.dateFilter,
        incomeSummary: state.incomeSummary.incomeSummary,
        totalIncome: state.incomeSummary.totalIncome,
        showSecondCatSummary: state.incomeSummary.showSecondCatSummary,
        loading: state.incomeSummary.loading,
        categoryType: state.incomeSummary.categoryType,
        location: state.router.location.pathname + '?' + state.router.location.search
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    showSecondCategory: ShowSecondCategoryIncomeSummaryAction.creator,
    loadIncomeExpense: LoadChartAction.creator,
    loadIncomesByCategory: LoadChartAction.creator,
    onChangeSelectedCategory: OnChangeSelectedCategoryAction.creator,
    onChangeSelectedSecondCategory: OnChangeSelectedSecondCategoryAction.creator
};


// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(IncomeSummary);
