import IStoreState from '../../../state/IStoreState';
import { connect } from 'react-redux';
import DriverLocator, { IPropsFromState, IPropsFromDispatch } from './DriverLocator';
import { SelectedDriverAction } from 'src/state/contexts/landing/Actions';
import { SaveOrderAction } from 'src/state/contexts/order/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        pickupPlaceName: state.landing.tripOverview?.pickupPlace.split(' ')[0] ?? "",
        dropoffCustomerName: state.landing.selectedCustomer?.firstName ?? "",
        pickupLocation: state.landing.pickupLocation,
        selectedDriver: state.landing.selectedDriver,
        selectedCustomer: state.landing.selectedCustomer,
        loading: state.order.saveOrderLoading,
        orderSavedStatus: state.order.saveOrderStatus
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    selectedDriverAssigned: SelectedDriverAction.creator,
    saveOrder: SaveOrderAction.creator
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(DriverLocator);
