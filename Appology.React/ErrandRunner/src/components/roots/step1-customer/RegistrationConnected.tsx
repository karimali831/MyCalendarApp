import IStoreState from '../../../state/IStoreState';
import { connect } from 'react-redux';
import Registration, { IPropsFromState, IPropsFromDispatch } from './Registration';
import { LoadStakeholdersAction, SelectedCustomerAction, SelectedDriverAction, ToggleAlertAction } from 'src/state/contexts/landing/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        activeStep: state.landing.activeStep,
        stakeholders: state.landing.stakeholders,
        filter: state.landing.filter,
        loading: state.landing.loading,
        selectedDriver: state.landing.selectedDriver
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    searchStakeholder: LoadStakeholdersAction.creator,
    selectedCustomerChange: SelectedCustomerAction.creator,
    selectedDriverChange: SelectedDriverAction.creator,
    handleAlert: ToggleAlertAction.creator
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(Registration);
