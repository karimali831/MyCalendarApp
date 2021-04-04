import IStoreState from '../../../../state/IStoreState';
import { connect } from 'react-redux';
import PickupLocation, { IPropsFromState, IPropsFromDispatch } from './PickupLocation';
import { DistanceMatrixAction, LoadPlacesAction, PlaceAction, SearchStoreAction, SelectedServiceAction, SetActiveStepAction } from 'src/state/contexts/landing/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        customer: state.landing.selectedCustomer,
        selectedService: state.landing.selectedService,
        selectedStore: state.landing.pickupPlace,
        searchStore: state.landing.searchStore,
        places: state.landing.places
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    onStoreChange: DistanceMatrixAction.creator,
    onPlaceChange: PlaceAction.creator,
    searchStoreChange: SearchStoreAction.creator,
    selectedServiceChange: SelectedServiceAction.creator,
    loadPlaces: LoadPlacesAction.creator,
    goToStep: SetActiveStepAction.creator
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(PickupLocation);
