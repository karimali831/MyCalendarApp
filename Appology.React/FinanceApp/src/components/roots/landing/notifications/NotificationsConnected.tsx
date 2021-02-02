import IStoreState from '../../../../state/IStoreState';
import { connect } from 'react-redux';
import Notifications, { IPropsFromState, IPropsFromDispatch } from './Notifications';
import { LoadNotificationsAction } from 'src/state/contexts/landing/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        notifications: state.notification.notifications,
        type: state.notification.type,
        loading: state.notification.loading

    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    loadNotifications: LoadNotificationsAction.creator
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(Notifications);
