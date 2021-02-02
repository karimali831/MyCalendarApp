import { connect } from 'react-redux';
import IStoreState from "../../../../../state/IStoreState";
import { FilterChangedAction } from 'src/state/contexts/landing/Actions';
import SelectionRefinementForSpendingSummary, { IPropsFromState, IPropsFromDispatch } from '../SelectionRefinementForSpendingSummary';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        filter: state.spendingSummary.categoryFilter,
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    filterChanged: FilterChangedAction.creator
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(SelectionRefinementForSpendingSummary);