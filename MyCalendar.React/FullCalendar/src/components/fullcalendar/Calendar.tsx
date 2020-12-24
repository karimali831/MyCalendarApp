import * as React from 'react';
import FullCalendar, { DateSelectArg, EventClickArg } from '@fullcalendar/react'
import dayGridPlugin from '@fullcalendar/daygrid'
import timeGridPlugin from '@fullcalendar/timegrid';
import listPlugin from '@fullcalendar/list';
import interactionPlugin from "@fullcalendar/interaction"; 
import { api, IEventRequest, IEventResponse } from 'src/Api/Api';
import { IEvent, IEventDateSet } from 'src/models/IEvent';
import bootstrapPlugin from '@fullcalendar/bootstrap';
import './Calendar.css'
import { IUserCalendar } from 'src/models/IUserCalendar';
import { SidebarMenu } from '../menu/SidebarMenu';
import { SaveEvent } from './SaveEvent';
import { EventLoader } from '../utils/Loader';
import { UserActivity } from './Activity';

export interface IOwnProps {

}

export interface IOwnState {
    loading: boolean,
    request: IEventRequest,
    pinSidebar: boolean,
    showCurrentActivity: boolean,
    showViewsMenu: boolean,
    showSaveEvent: boolean,
    events: IEvent[],
    userCalendars: IUserCalendar[],
    selectedCalendarIds: number[],
    userId: string,
    currentActivity: string[]
}

export default class Calendar extends React.Component<IOwnProps, IOwnState> {

    private calendarRef = React.createRef<FullCalendar>();

    constructor(props: IOwnProps) {
        super(props);
        this.state = {
            loading: false,
            request: {
                calendarIds: [],
                year: [new Date().getFullYear()],
                month: [new Date().getMonth() + 1]
            },
            pinSidebar: true,
            showCurrentActivity: true,
            showViewsMenu: false,
            showSaveEvent: false,
            currentActivity: [],
            events: [],
            userCalendars: [],
            selectedCalendarIds: [],
            userId: "",
        };
    }

    public componentDidMount = () => {
        this.getEvents();
    }

    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (prevState.selectedCalendarIds.length > 0 && this.calendarRef.current !== null) {
            if (JSON.stringify(this.state.selectedCalendarIds) !== JSON.stringify(prevState.selectedCalendarIds)) {
                this.getEvents();
            }
        }

        if (prevState.request.month.length > 0) {
            if (JSON.stringify(this.state.request.month) !== JSON.stringify(prevState.request.month) ||
                JSON.stringify(this.state.request.year) !== JSON.stringify(prevState.request.year)) {
                this.getEvents();
            }
        }
    }

    public render() {   
        const userCalendars = 
            this.state.userCalendars
                .filter(uc => uc.userCreatedId === this.state.userId);

        return (
            <div>
                <SidebarMenu 
                    pinSidebar={this.state.pinSidebar} 
                    loading={this.state.loading} 
                    userId={this.state.userId} 
                    userCalendars={this.state.userCalendars}  
                    calendarSelected={this.calendarSelected} 
                    viewSelected={this.changeView}
                />
                <div className={this.state.pinSidebar ? "calendar-margin" : ""}>
                    <UserActivity 
                        activity={this.state.currentActivity}
                        onClose={this.showCurrentActivity} 
                        display={this.state.showCurrentActivity && this.state.currentActivity.length > 0}  />
                    {
                        this.state.showSaveEvent ? 
                            <SaveEvent 
                                userCalendars={userCalendars} 
                                userId={this.state.userId} 
                                saveEventChange={(show: boolean) => this.saveEventChange(show)} /> 
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
                            activity: {
                                icon: `fa fa fa-user${this.state.showCurrentActivity ? "-minus" : "-clock"}`,
                                click: this.showCurrentActivity
                            }
                        }}
                        headerToolbar={{
                            left: "toggle,prev,next",
                            center: "title",
                            right: "today,activity"
                        }}
                        views={{
                            dayGridMonth: {
                                titleFormat:  { year: 'numeric', month: 'short' }
                            }
                        }}
                        select={this.handleDateSelect}
                        selectable={true}
                        loading={() => this.state.loading}
                        editable={true}
                        events={this.state.events}
                        height={650}
                        datesSet={(dateInfo) => this.datesSet(dateInfo)}
                        dayMaxEventRows={4}
                        // dateClick={this.handleDateClick}
                        eventClick={this.handleEventClick}
                        ref={this.calendarRef} />
                </div>
            </div>
        )
    }


    private showCurrentActivity = () => {
        this.setState({ showCurrentActivity: !this.state.showCurrentActivity })
    }

    private handleDateSelect = (date: DateSelectArg) => {

        this.saveEventChange(true)

    }

    private saveEventChange = (show: boolean) => {
        this.setState({ showSaveEvent: show })
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
            currentActivity: calendar.currentActivity
        })
    }

    // private handleDateClick = (arg: DateClickArg) => { 
    //     alert(arg.dateStr)
    // }

    private handleEventClick = (arg: EventClickArg) => { 
        alert(JSON.stringify(arg.event))
    }
}