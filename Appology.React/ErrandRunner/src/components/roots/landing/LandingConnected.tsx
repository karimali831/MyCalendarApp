import IStoreState from '../../../state/IStoreState';
import { connect } from 'react-redux';
import Landing, { IPropsFromState, IPropsFromDispatch } from './Landing';
import { ResetOrderAction, ToggleAlertAction, UpdateConfigAction } from 'src/state/contexts/landing/Actions';
import { Variant } from '@appology/react-components';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        selectedCustomer: state.landing.selectedCustomer,
        selectedOrder: state.order.order,
        config: state.landing.config,
        pinSidebar: state.order.pinSidebar,
        step: state.landing.activeStep,
        alertTxt: state.landing.alertTxt,
        alertVariant: state.landing.alertVariant ?? Variant.Success,
        alertTimeout: state.landing.alertTimeout
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    updateConfig: UpdateConfigAction.creator,
    resetOrder: ResetOrderAction.creator,
    handleAlert: ToggleAlertAction.creator
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(Landing);
