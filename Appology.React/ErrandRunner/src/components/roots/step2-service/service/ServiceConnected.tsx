import IStoreState from '../../../../state/IStoreState';
import { connect } from 'react-redux';
import Service, { IPropsFromState, IPropsFromDispatch } from './Service';
import { SelectedServiceAction } from 'src/state/contexts/landing/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        selectedCustomer: state.landing.selectedCustomer,
        selectedService: state.landing.selectedService,
        order: state.order.order
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    selectedServiceChange: SelectedServiceAction.creator
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(Service);
