import { connect } from 'react-redux';
import OrderDetails, { IPropsFromState, IPropsFromDispatch } from './OrderDetails';
import { SetDriverStep4Action, ToggleConfigAction, UpdateOrderAction } from 'src/state/contexts/order/Actions';
import IStoreState from 'src/state/IStoreState';
import { ResetOrderAction, ToggleAlertAction } from 'src/state/contexts/landing/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        tripOverview: state.landing.tripOverview,
        order: state.order.orderForm,
        orderOverview: state.order.orderOverview,
        config: state.landing.config,
        pinSidebar: state.order.pinSidebar
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    toggleConfig: ToggleConfigAction.creator,
    updateOrder: UpdateOrderAction.creator,
    setDriverStep4: SetDriverStep4Action.creator,
    handleAlert: ToggleAlertAction.creator,
    resetOrder: ResetOrderAction.creator
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(OrderDetails);
