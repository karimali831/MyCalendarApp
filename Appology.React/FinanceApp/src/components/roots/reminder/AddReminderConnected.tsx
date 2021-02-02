import IStoreState from '../../../state/IStoreState';
import { connect } from 'react-redux';
import AddReminder, { IPropsFromState } from './AddReminder';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        selectedCat: state.common.selectedCat
    });

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps)(AddReminder);
