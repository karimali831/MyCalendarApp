import * as React from 'react';
import FullCalendar, { EventClickArg } from '@fullcalendar/react'
import dayGridPlugin from '@fullcalendar/daygrid'
import timeGridPlugin from '@fullcalendar/timegrid';
import listPlugin from '@fullcalendar/list';
import interactionPlugin, { DateClickArg } from "@fullcalendar/interaction"; // needed for dayClick
import { api, IEventResponse } from 'src/Api/Api';
import { IEvent } from 'src/models/IEvent';
import { Load } from '@appology/react-components';
import bootstrapPlugin from '@fullcalendar/bootstrap';
import './Calendar.css'
import { IUserCalendar } from 'src/models/IUserCalendar';
import { SidebarMenu } from '../menu/SidebarMenu';

export interface IOwnProps {

}

export interface IOwnState {
    loading: boolean,
    loadingCalendars: boolean,
    pinSidebar: boolean,
    showViewsMenu: boolean,
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
            loadingCalendars: false,
            pinSidebar: false,
            showViewsMenu: false,
            currentActivity: [],
            events: [],
            userCalendars: [],
            selectedCalendarIds: [],
            userId: "",
        };
    }

    public componentDidMount = () => {
        this.getEvents([]);
    }

    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (prevState.selectedCalendarIds.length > 0) {
            if (JSON.stringify(this.state.selectedCalendarIds) !== JSON.stringify(prevState.selectedCalendarIds)) {
                this.getEvents(this.state.selectedCalendarIds, true);
            }
        }
    }

    public render() {
        return (
            <div>
                {this.state.loading ? 
                    <Load withBackground={true} /> :
                    <>
                        <SidebarMenu 
                            expanded={true} 
                            pinSidebar={this.state.pinSidebar} 
                            loading={this.state.loadingCalendars} 
                            userId={this.state.userId} 
                            userCalendars={this.state.userCalendars}  
                            calendarSelected={this.calendarSelected} 
                            viewSelected={this.changeView}
                        />
                        <div className={this.state.pinSidebar ? "calendar-margin" : ""}>
                            <FullCalendar
                                plugins={[ dayGridPlugin, timeGridPlugin, listPlugin, interactionPlugin, bootstrapPlugin]}
                                initialView="dayGridMonth"
                                displayEventTime={true}
                                eventDisplay="block"
                                customButtons={{
                                    toggle: {
                                        icon: "fa fa fa-bars",
                                        click: this.toggleMenu
                                        
                                    }
                                }}
                                headerToolbar={{
                                    left: "toggle,prev,next",
                                    center: "title",
                                    right: "today"
                                }}
                                selectable={true}
                                loading={() => this.state.loading}
                                editable={true}
                                events={this.state.events}
                                height={600}
                                dateClick={this.handleDateClick}
                                eventClick={this.handleEventClick}
                                ref={this.calendarRef} />
                       </div>
                    </>
                }
            </div>
        )
    }

    private toggleMenu = () => {
        this.setState({ pinSidebar: !this.state.pinSidebar })
    }

    private calendarSelected = (selectedCalendar: IUserCalendar) => {
        let selectedCalendars = [...this.state.selectedCalendarIds, selectedCalendar.id];

        if (this.state.selectedCalendarIds.includes(selectedCalendar.id)) {
            selectedCalendars = selectedCalendars.filter(uc => uc !== selectedCalendar.id);
        } 

        this.setState({
            loadingCalendars: true,
            selectedCalendarIds: selectedCalendars
        })
    }

    private changeView = (viewName: string):void=> {
        if (this.calendarRef.current !== null)
        {
            this.calendarRef.current
                .getApi()
                .changeView(viewName)
        }
    }

    private getEvents = (calendarIds: number[], loadingCalendars: boolean = false) => {
        this.setState({ 
            loading: loadingCalendars ? false : true, 
            loadingCalendars: loadingCalendars 
        })

        api.events(calendarIds)
            .then(e => this.eventsSuccess(e));
    }

    private eventsSuccess = (calendar: IEventResponse) => {
        this.setState({ 
            loading: false,
            loadingCalendars: false,
            events: calendar.events,
            userCalendars: calendar.userCalendars,
            selectedCalendarIds: calendar.userCalendars.filter(o => o.selected).map(c=> c.id),
            userId: calendar.userId,
            currentActivity: calendar.currentActivity
        })
    }

    private handleDateClick = (arg: DateClickArg) => { // bind with an arrow function
        alert(arg.dateStr)
    }

    private handleEventClick = (arg: EventClickArg) => { // bind with an arrow function
        alert(JSON.stringify(arg.event))
    }
}