import IStoreState from '../../../../state/IStoreState';
import { connect } from 'react-redux';
import ExistingOrders, { IPropsFromState, IPropsFromDispatch } from './ExistingOrders';
import { SelectedOrderAction } from 'src/state/contexts/order/Actions';
import { ResetOrderAction, SelectedDriverAction, ToggleAlertAction } from 'src/state/contexts/landing/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        selectedCustomer: state.landing.selectedCustomer,
        order: state.order.order
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    resetOrder: () => ResetOrderAction,
    selectedDriverChange: SelectedDriverAction.creator,
    selectedOrderChange: SelectedOrderAction.creator,
    handleAlert: ToggleAlertAction.creator
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(ExistingOrders);
