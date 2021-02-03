import * as React from 'react';
import { Load } from '../../../base/Loader';
import { faTimes } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { LoadNotificationsAction } from 'src/state/contexts/landing/Actions';
import { commonApi } from 'src/Api/CommonApi';
import { priorityBadge } from 'src/components/utils/Utils';
import { IReminder, IReminderNotification } from 'src/models/IReminder';
import { ReminderType } from 'src/enums/ReminderType';
import Button from 'react-bootstrap/Button'
import Badge from 'react-bootstrap/Badge'

export interface IPropsFromState {
    notifications?: IReminderNotification,
    loading: boolean,
    type: string
}

export interface IPropsFromDispatch {
    loadNotifications: () => LoadNotificationsAction
}

export interface IOwnState {
    deletedReminderId?: number,
    loadingDeleted: boolean,
    loading: boolean,
    showReminders: ReminderType
}

type AllProps = IPropsFromState & IPropsFromDispatch;

export default class Notifications extends React.Component<AllProps, IOwnState> {

    constructor(props: AllProps) {
        super(props);
        this.state = { 
            loadingDeleted: false,
            loading: this.props.loading,
            deletedReminderId: undefined,
            showReminders: ReminderType.DueToday
        };
    }

    public componentDidUpdate(prevProps: AllProps, prevState: IOwnState) {
        if (prevState.deletedReminderId !== this.state.deletedReminderId) {
            this.props.loadNotifications()
        }
    }

    public render() {
        const { loading, notifications } = this.props;

        if (loading) {
            return <Load text="Loading notifications..." />
        }

        if (this.state.loadingDeleted) {
            return <Load />
        }

        if (notifications === undefined) {
            return;
        }

        return (
            <div>
                {
                    notifications.overDueReminders.length > 0 ? 
                        <Button variant="danger" onClick={() => this.showReminders(ReminderType.Overdue)}>
                            <span className="notificationLabel">Overdue</span> <Badge variant="light">{notifications.overDueReminders.length}</Badge>
                        </Button>
                    : null
                }
                {
                    notifications.dueTodayReminders.length > 0 ? 
                        <Button variant="warning" onClick={() => this.showReminders(ReminderType.DueToday)}>
                            <span className="notificationLabel">Due Today</span> <Badge variant="light">{notifications.dueTodayReminders.length}</Badge>
                        </Button>
                    : null
                }
                {
                    notifications.upcomingReminders.length > 0 ? 
                        <Button variant="info" onClick={() => this.showReminders(ReminderType.Upcoming)}>
                            <span className="notificationLabel">Upcoming</span> <Badge variant="light">{notifications.upcomingReminders.length}</Badge>
                        </Button>
                    : null
                }
                {
                    notifications.alerts.length > 0 ? 
                        <Button variant="dark" onClick={() => this.showReminders(ReminderType.Alert)}>
                            <span className="notificationLabel">Alerts</span> <Badge variant="secondary">{notifications.alerts.length}</Badge>
                        </Button>
                    : null
                }
                <>
                {
                    this.state.showReminders === ReminderType.Overdue && notifications.overDueReminders.length > 0 ? 
                        <div className="alert alert-danger d-flex flex-row" role="alert">
                            {
                                notifications.overDueReminders.length > 0 ? 
                                    <div style={{display: 'block'}}>
                                    {
                                        notifications.overDueReminders.map(r =>
                                            this.notificationCard(r, "overdue by " + r.daysUntilDue + " day" + (r.daysUntilDue === -1 ? "" : "s"))
                                        )
                                    }
                                    </div>
                                : null
                            }
                        </div>
                    : null    
                }
                {
                    this.state.showReminders === ReminderType.DueToday && notifications.dueTodayReminders.length > 0 ? 
                        <div className="alert alert-warning d-flex flex-row" role="alert">
                            {
                                notifications.dueTodayReminders.length > 0  ? 
                                    <div style={{display: 'block'}}>
                                    {
                                        notifications.dueTodayReminders.map(r =>
                                            this.notificationCard(r)
                                        )
                                    }
                                    </div>
                                : null
                            }
                        </div>
                : null    
                }
                {
                    this.state.showReminders === ReminderType.Upcoming && notifications.upcomingReminders.length > 0 ? 
                        <div className="alert alert-info d-flex flex-row" role="alert">
                            {
                                notifications.upcomingReminders.length > 0 ? 
                                    <div style={{display: 'block'}}>
                                    {
                                        notifications.upcomingReminders.map(r =>
                                            this.notificationCard(r, "due in " + r.daysUntilDue + " day" + (r.daysUntilDue === 1 ? "" : "s"))
                                        )
                                    }
                                    </div>
                                : null
                            }
                        </div>
                    : null    
                }
                {
                    this.state.showReminders === ReminderType.Alert && notifications.alerts.length > 0 ? 
                        <div className="alert alert-dark d-flex flex-row" role="alert">
                            {
                                notifications.alerts.length > 0 ? 
                                    <div style={{display: 'block'}}>
                                    {
                                        notifications.alerts.map(r =>
                                            this.notificationCard(r)
                                        )
                                    }
                                    </div>
                                : null
                            }
                        </div>
                    : null    
                }
                </>
                <div className="card-group">
                    <div className="card">
                        <div className="card-body">
                        <h5 className="card-title">Summary Info</h5>
                            <p className="card-text">
                                Estimated balance: {notifications.summary.estimatedBalance} <br />
                                Remaining cash: {notifications.summary.remainingCash} <br />
                                Accrued savings: {notifications.summary.accruedSavings} 
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        )
    }

    private notificationCard = (reminder: IReminder, text: string = "") => {
        let badgeLabel = reminder.category;

        if (text !== "" && reminder.sort !== 0) {
            badgeLabel = `${badgeLabel} ${text}`;
        }

        return (
            <div key={reminder.id}>
                <span onClick={() => this.deleteReminder(reminder.id)}>
                    <FontAwesomeIcon icon={faTimes} />
                </span> 
                &nbsp;
                {priorityBadge(reminder.priority, badgeLabel)} &nbsp;
                <span>{reminder.notes}</span>&nbsp;
            </div>
        )
    }

    private showReminders = (reminderType: ReminderType) => {
        this.setState({ ...this.state,  showReminders: reminderType })
    }

    private deleteReminder = (id: number) => {
        this.setState({ ...this.state, loadingDeleted: true, deletedReminderId: id })
  
        commonApi.remove(id, "Reminders")
          .then(() => this.deleteReminderSuccess());
    }

    private deleteReminderSuccess = () => {
        this.setState({ ...this.state,
            ...{ 
                loadingDeleted: false
            }
        }) 
    }

}