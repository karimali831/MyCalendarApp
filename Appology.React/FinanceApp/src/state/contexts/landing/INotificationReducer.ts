
import INotificationsState, { NotificationState } from './INotificationState';
import { Reducer } from 'redux';
import { LandingSummaryActions, LandingSummaryActionTypes } from './Actions';

const NotificationReducer: Reducer<INotificationsState, LandingSummaryActions> =
    (state = NotificationState.intialState, action) => {
        switch (action.type) {
            case LandingSummaryActionTypes.LoadNotificationsSuccess:
                return {
                    ...state,
                    ...{
                        notifications: action.notifications,
                        loading: false
                    }
                }
                

            default:
                return state;
        }
    }

export default NotificationReducer;