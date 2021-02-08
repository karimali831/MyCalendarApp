import IStoreState from '../../../../state/IStoreState';
import { connect } from 'react-redux';
import PickupLocation, { IPropsFromState, IPropsFromDispatch } from './PickupLocation';
import { DistanceMatrixAction } from 'src/state/contexts/landing/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        customer: state.landing.selectedCustomer
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    onStoreChange: DistanceMatrixAction.creator
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(PickupLocation);
