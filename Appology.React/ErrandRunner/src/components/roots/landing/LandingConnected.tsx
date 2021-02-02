import IStoreState from '../../../state/IStoreState';
import { connect } from 'react-redux';
import Landing, { IPropsFromState, IPropsFromDispatch } from './Landing';
import { ResetOrderAction, SelectedDriverAction, SelectedServiceAction, UpdateConfigAction } from 'src/state/contexts/landing/Actions';
import { SelectedOrderAction } from 'src/state/contexts/order/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        tripOverview: state.landing.tripOverview,
        selectedCustomer: state.landing.selectedCustomer,
        selectedService: state.landing.selectedService,
        config: state.landing.config,
        pinSidebar: state.order.pinSidebar,
        stakeholders: state.landing.stakeholders,
        filter: state.landing.filter,
        step: state.landing.activeStep,
        order: state.order.order
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    updateConfig: UpdateConfigAction.creator,
    resetOrder: ResetOrderAction.creator,
    selectedDriverChange: SelectedDriverAction.creator,
    selectedOrderChange: SelectedOrderAction.creator,
    selectedServiceChange: SelectedServiceAction.creator,
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(Landing);
