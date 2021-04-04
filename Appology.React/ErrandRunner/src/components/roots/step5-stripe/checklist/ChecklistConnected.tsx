import { connect } from 'react-redux';
import OrderChecklist, { IPropsFromState, IPropsFromDispatch } from './Checklist';
import IStoreState from 'src/state/IStoreState';
import { DispatchOrderAction, SaveOrderAction } from 'src/state/contexts/order/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        order: state.order.order,
        deliveryDate: state.order.deliveryDate,
        timeslot: state.order.timeslot,
        saveOrderStatus: state.order.saveOrderStatus,
        actions: state.order.orderStatus,
        orderStatus: state.order.orderStatus
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    dispatchOrder: DispatchOrderAction.creator,
    saveOrder: SaveOrderAction.creator
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(OrderChecklist);
