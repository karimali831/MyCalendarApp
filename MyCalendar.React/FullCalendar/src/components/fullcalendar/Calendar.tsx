import * as React from 'react';
import FullCalendar, { DateSelectArg, EventApi, EventClickArg, EventContentArg } from '@fullcalendar/react'
import dayGridPlugin from '@fullcalendar/daygrid'
import timeGridPlugin from '@fullcalendar/timegrid';
import listPlugin from '@fullcalendar/list';
import interactionPlugin from "@fullcalendar/interaction"; 
import { api, IActivityResponse, IEventRequest, IEventResponse } from 'src/Api/Api';
import { IEvent, IEventDateSet, IEventSelect } from 'src/models/IEvent';
import bootstrapPlugin from '@fullcalendar/bootstrap';
import './Calendar.css'
import { IUserCalendar } from 'src/models/IUserCalendar';
import { SidebarMenu } from '../menu/SidebarMenu';
import { SaveEvent } from './SaveEvent';
import { EventLoader } from '../utils/Loader';
import { UserActivity } from './Activity';
import BootstrapAlert from 'react-bootstrap/Alert';
import { Modal } from 'react-bootstrap';
import { FaCheck } from 'react-icons/fa';
import { EditEvent } from './EditEvent';
import * as moment from 'moment';
import { UserAvatar } from './UserAvatar';
import { IActivity } from 'src/models/IActivity';

export interface IOwnProps {

}

export interface IOwnState {
    loading: boolean,
    request: IEventRequest,
    pinSidebar: boolean,
    showCurrentActivity: boolean,
    retainSelected?: boolean,
    retainSelection: boolean,
    alert: boolean,
    alertMsg: string,
    showViewsMenu: boolean,
    eventSelect?: IEventSelect,
    events: IEvent[],
    userCalendars: IUserCalendar[],
    selectedCalendarIds: number[],
    userId: string,
    currentActivity: IActivity[],
    showAvatars: boolean
}

export default class Calendar extends React.Component<IOwnProps, IOwnState> {

    private calendarRef = React.createRef<FullCalendar>();
    private currentMonth = Number(moment().format('M'));
    private currentYear = Number(moment().format('Y'));


    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            loading: false,
            request: {
                calendarIds: [],
                year: 
                    this.currentMonth === 12 ? [
                        this.currentYear, 
                        Number(moment().add(1, "years").format('Y'))
                    ] :
                    this.currentMonth === 1 ? [
                        this.currentYear, 
                        Number(moment().subtract(1, "years").format('Y'))
                    ] : [
                        this.currentYear
                    ],
                month: [
                    Number(moment().subtract(1, "months").format('M')), 
                    Number(moment().format('M')), 
                    Number(moment().add(1, "months").format('M'))
                ]
            },
            pinSidebar: false,
            showCurrentActivity: true,
            retainSelected: undefined,
            retainSelection: false,
            alert: false,
            alertMsg: "",
            showViewsMenu: false,
            eventSelect: undefined,
            currentActivity: [],
            events: [],
            userCalendars: [],
            selectedCalendarIds: [],
            userId: "",
            showAvatars: false
        };
    }

    public componentDidMount = () => {
        this.getEvents();
    }

    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (prevState.selectedCalendarIds.length > 0 && this.calendarRef.current !== null) {
            if (JSON.stringify(this.state.selectedCalendarIds) !== JSON.stringify(prevState.selectedCalendarIds)) {
                {this.state.userCalendars.filter(c => c.userCreatedId !== this.state.userId).map(c =>  
                    this.setState({ showAvatars: c.selected })
                )}
                
                this.getEvents();
            }
        }

        if (prevState.request.month.length > 0) {
            if (JSON.stringify(this.state.request.month) !== JSON.stringify(prevState.request.month) ||
                JSON.stringify(this.state.request.year) !== JSON.stringify(prevState.request.year)) {
                this.getEvents();
            }
        }

        if (this.state.retainSelected !== prevState.retainSelected) {
            this.saveRetainSelection();
        }

        if (this.state.alert !== prevState.alert) {
            setTimeout(() => { 
                this.setState(() => ({alert: false}))
              }, 2000);
        }
    }

    public render() {   
        const userCalendars = 
            this.state.userCalendars
                .filter(uc => uc.userCreatedId === this.state.userId);

        return (
            <div>
                <Modal onHide={() => this.handleClose} show={this.state.alert}>
                    <BootstrapAlert variant="success">
                        <FaCheck /> {this.state.alertMsg}
                    </BootstrapAlert>
                </Modal>
                <SidebarMenu 
                    pinSidebar={this.state.pinSidebar} 
                    loading={this.state.loading} 
                    userId={this.state.userId} 
                    userCalendars={this.state.userCalendars}  
                    retainSelection={this.state.retainSelection}
                    retainSelected={this.retainSelected}
                    calendarSelected={this.calendarSelected} 
                    viewSelected={this.changeView}
                />  
                <div className={this.state.pinSidebar ? "calendar-margin" : ""}>
                    <UserActivity 
                        activity={this.state.currentActivity}
                        onClose={this.showCurrentActivity} 
                        display={this.state.showCurrentActivity && this.state.currentActivity.length > 0}  />
                    {
                        this.state.eventSelect?.dateStart  || (this.state.eventSelect?.eventEdit && this.state.eventSelect?.eventEdit.userId === this.state.userId) ? 
                            <SaveEvent 
                                eventSelect={this.state.eventSelect}
                                userCalendars={userCalendars} 
                                userId={this.state.userId} 
                                onSaveChange={(loading: boolean, event: IEvent) => this.saveEventChange(loading, event)}
                                onCancelChange={() => this.resetSelect()} /> 
                            : null
                    }
                    {
                        this.state.eventSelect?.event !== undefined && this.state.eventSelect.event.userId === this.state.userId ?
                            <EditEvent
                                event={this.state.eventSelect.event}
                                onEditChange={this.handleEditDateSelect}
                                onDeleteChange={(eventId: string) => this.handleEventDelete(eventId)}
                                onCancelChange={() => this.resetSelect()} />
                        : null
                   
                    }      
                    <EventLoader display={this.state.loading} />
                    <FullCalendar
                        plugins={[ dayGridPlugin, timeGridPlugin, listPlugin, interactionPlugin, bootstrapPlugin]}
                        initialView="dayGridMonth"
                        customButtons={{
                            toggle: {
                                icon: "fa fa fa-bars",
                                click: this.toggleMenu
                            },
                            avatars: {
                                icon: `fa fa ${this.state.showAvatars ? "fa-user-slash" : "fa-user"}`,
                                click: this.showAvatars
                            },
                            refresh: {
                                icon: "fa fa fa-sync-alt",
                                click: this.getEvents
                            }
                        }}
                        headerToolbar={{
                            left: "toggle,prev,next",
                            center: "title",
                            right: "today,avatars,refresh"
                        }}
                        views={{
                            dayGridMonth: {
                                titleFormat:  { year: 'numeric', month: 'short' }
                            }
                        }}
                        eventContent={(c: EventContentArg) => this.renderEventContent(c)}
                        select={(date: DateSelectArg) => this.handleDateSelect(date)}
                        selectable={true}
                        loading={() => this.state.loading}
                        events={this.state.events}
                        longPressDelay={200}
                        height={650}
                        eventDisplay="block"
                        datesSet={(dateInfo) => this.datesSet(dateInfo)}
                        dayMaxEventRows={4}
                        eventClick={(e: EventClickArg) => this.handleEventSelect(e.event)}
                        ref={this.calendarRef} />
                </div>
            </div>
        )
    }

    private showAvatars = () => {
        this.setState({ showAvatars: !this.state.showAvatars })
    }

    private renderEventContent = (eventInfo: EventContentArg) => {
        const event = this.state.events.filter(e => e.id === eventInfo.event.id)[0];
        const content = <><b>{eventInfo.timeText}</b> {eventInfo.event.title}</>

        if(this.state.showAvatars && event) {
            return <UserAvatar avatar={event.avatar} content={content} />
        } else {
            return content;
        }
    }

    private handleClose = () => { return; }

    private showCurrentActivity = () => {
        this.setState({ showCurrentActivity: !this.state.showCurrentActivity })
    }

    private handleDateSelect = (dates?: DateSelectArg) => {
        this.setState({ 
            eventSelect: { ...this.state.eventSelect,    
                dateStart: dates?.start,
                dateEnd: dates?.end
            }
        })
    }

    private resetSelect = () => {
        this.setState({ eventSelect: undefined })
    }

    private handleEditDateSelect = () => {
        this.setState({
             eventSelect: { ...this.state.eventSelect,  
                eventEdit: this.state.eventSelect?.event
            }
        })
    }

    private toggleMenu = () => {
        this.setState({ pinSidebar: !this.state.pinSidebar })
    }

    private calendarSelected = (selectedCalendar: IUserCalendar) => {
        let selectedCalendars = [...this.state.selectedCalendarIds, selectedCalendar.id];

        if (this.state.selectedCalendarIds.includes(selectedCalendar.id)) {
            selectedCalendars = selectedCalendars.filter(uc => uc !== selectedCalendar.id);
        } 

        this.setState({ selectedCalendarIds: selectedCalendars })
    }

    private changeView = (viewName: string):void=> {
        if (this.calendarRef.current !== null)
        {
            this.calendarRef.current
                .getApi()
                .changeView(viewName)
        }
    }

    private datesSet = (dates: IEventDateSet) => {
        if (this.calendarRef.current !== null) {
            const month = this.calendarRef.current.getApi().view.currentStart.getMonth() + 1;
            const year = this.calendarRef.current.getApi().view.currentStart.getFullYear();

            if (!this.state.request.month.includes(month)) {
                this.setState({ 
                    request: { ...this.state.request,    
                        month: [...this.state.request.month, month]
                    }
                })
            }

            if (!this.state.request.year.includes(year)) {
                this.setState({ 
                    request: { ...this.state.request,    
                        year: [...this.state.request.year, year]
                    }
                })
            }
        }
    }

    private retainSelected = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.setState({ 
            loading: true,
            retainSelected: e.target.checked, 
            retainSelection: e.target.checked 
        })
    }

    private saveRetainSelection = () => {
        api.retainSelection(!this.state.retainSelected ? null : this.state.selectedCalendarIds)
            .then(s => this.showAlert("The calendars selected are now retained"));
    }

    private showAlert = (msg: string) => {
        this.setState({
            alert: true,
            alertMsg: msg,
            loading: false
        })
    }

    private getEvents = () => {
        this.setState({ 
            events: [],
            loading: true
        })

        const request: IEventRequest = {
            month: this.state.request.month,
            year: this.state.request.year,
            calendarIds: this.state.selectedCalendarIds
        }

        api.events(request)
            .then(e => this.eventsSuccess(e));
        
    }

    private eventsSuccess = (calendar: IEventResponse) => {
        this.setState({ 
            loading: false,
            events: calendar.events,
            userCalendars: calendar.userCalendars,
            selectedCalendarIds: calendar.userCalendars.filter(o => o.selected).map(c=> c.id),
            userId: calendar.userId,
            retainSelection: calendar.retainSelection,
            showAvatars: calendar.userCalendars.some(c => c.userCreatedId !== calendar.userId && c.selected)
        })

        this.loadActivity();

    }

    private loadActivity = () => {
        api.activity()
            .then(e => this.loadActivitySuccess(e))
    }

    private loadActivitySuccess = (activity: IActivityResponse) => {
        this.setState({ 
            currentActivity: activity.activity
        })
    }

   
    private saveEventChange = (loading: boolean, event: IEvent) => {
        this.resetSelect()
        this.setState({ loading: loading})

        if (!loading) {
            this.showAlert("Event successfully saved");

            // update event
            if (this.state.events.some(el => el.id === event.id)) {
                this.setState(prevState => ({       
                    events: prevState.events.map(el => 
                        (el.id === event.id ? event : el)
                    ) 
                }))
            }
            // new event
            else{
                this.setState({ events: [...this.state.events, event] })
            }

            this.loadActivity();
        }
    } 

    private handleEventDelete = (eventId: string) => {
        this.resetSelect();
        this.setState({ loading: true })

        api.deleteEvent(eventId)
            .then(s => this.deleteEventSuccess(eventId, s));
    }

    private deleteEventSuccess = (eventId: string, status: boolean) => {
        this.setState({ loading: false })

        if (status) {
            this.showAlert("The event deleted successfully")
            this.setState({ 
                events: this.state.events.filter(e => e.id !== eventId)
            })
            this.loadActivity();
        }
        else{
            alert("An error occured");
        }
    }

    private handleEventSelect = (eventApi: EventApi) => {
        const e = this.state.events.filter(ev => ev.id === eventApi.id)[0];
        const event = {
            id: e.id,
            calendarId: e.calendarId,
            reminder: e.reminder,
            title: e.title,
            startStr: e.startStr,
            endStr: e.endStr,
            allDay: e.allDay,
            tagId: e.tagId,
            description: e.description,
            tentative: e.tentative,
            eventUid: e.eventUid,
            alarm: e.alarm,
            duration: e.duration,
            userId: e.userId
        }
    
        this.setState({ 
            eventSelect: { ...this.state.eventSelect,  
                event: event
            }
         })
    }
}