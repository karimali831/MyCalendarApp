import { IReminderNotification } from 'src/models/IReminder';

export default interface INotificationState {
    notifications?: IReminderNotification,
    type: string,
    loading: boolean,
}

export class NotificationState {
    public static readonly intialState = {
        notifications: undefined,
        type: "",
        loading: true
    }
}