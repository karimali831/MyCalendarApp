import IStoreState from '../../../state/IStoreState';
import { connect } from 'react-redux';
import { Overview, IPropsFromState, IPropsFromDispatch } from './Overview';
import { ToggleAlertAction } from 'src/state/contexts/landing/Actions';
import { OrderPaidAction, OrderStatusAction, SetDeliveryDateAction } from 'src/state/contexts/order/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        deliveryDate: state.order.deliveryDate,
        timeslot: state.order.timeslot,
        order: state.order.order,
        orderOverview: state.order.orderOverview,
        saveOrderStatus: state.order.saveOrderStatus,
        orderStatus: state.order.orderStatus,
        stripeConfirmationPaymentId: state.order.stripePaymentConfirmationId
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    handleAlert: ToggleAlertAction.creator,
    setDeliveryDate: SetDeliveryDateAction.creator,
    orderStatusChange: OrderStatusAction.creator,
    orderPaid: OrderPaidAction.creator
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(Overview);
