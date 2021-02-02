import IStoreState from '../../../state/IStoreState';
import { connect } from 'react-redux';
import Spendings, { IPropsFromState, IPropsFromDispatch } from './Spendings';
import { LoadSpendingsAction } from 'src/state/contexts/spending/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        spendings: state.spending.spendings,
        loading: state.spending.loading
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    loadSpendings: LoadSpendingsAction.creator
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(Spendings);
