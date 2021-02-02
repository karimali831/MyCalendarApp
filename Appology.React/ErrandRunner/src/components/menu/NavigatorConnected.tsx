
import { connect } from 'react-redux';
import Navigator, { IPropsFromState, IPropsFromDispatch } from './Navigator';
import IStoreState from 'src/state/IStoreState';
import { SetActiveStepAction } from 'src/state/contexts/landing/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        activeStep: state.landing.activeStep,
        navigator: state.landing.navigator
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    setActiveStep: SetActiveStepAction.creator
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(Navigator);
