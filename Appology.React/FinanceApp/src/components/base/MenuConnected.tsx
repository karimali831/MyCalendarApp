import IStoreState from '../../state/IStoreState';
import { connect } from 'react-redux';
import Menu, { IPropsFromState } from './Menu';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        location: state.router.location.pathname.split('/')[3]
    });

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps)(Menu)