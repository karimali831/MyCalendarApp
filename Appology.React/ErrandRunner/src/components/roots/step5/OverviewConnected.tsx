import IStoreState from '../../../state/IStoreState';
import { connect } from 'react-redux';
import { Overview, IPropsFromState, IPropsFromDispatch } from './Overview';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        orderId: state.order.order?.orderId,
        orderOverview: state.order.orderOverview
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{

};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(Overview);
