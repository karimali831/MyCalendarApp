import IStoreState from '../../../state/IStoreState';
import { connect } from 'react-redux';
import Incomes, { IPropsFromState, IPropsFromDispatch } from './Incomes';
import { LoadIncomesAction } from 'src/state/contexts/income/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        incomes: state.income.incomes,
        loading: state.income.loading
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    loadIncomes: LoadIncomesAction.creator
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(Incomes);
