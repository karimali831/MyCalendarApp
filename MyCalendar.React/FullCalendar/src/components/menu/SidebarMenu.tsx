import * as React from 'react';
import SideNav, { NavItem, NavIcon, NavText } from '@trendmicro/react-sidenav';
import { FaCalendar, FaCalendarAlt, FaCalendarCheck, FaCalendarDay, FaCalendarWeek, FaSyncAlt } from 'react-icons/fa'
import ClickOutside from 'react-click-outside'
import '@trendmicro/react-sidenav/dist/react-sidenav.css';
import './SidebarMenu.css'
import { IUserCalendar } from 'src/models/IUserCalendar';
import { Load } from '@appology/react-components';

interface IOwnState {
    expanded: boolean,
    selected: string
}

interface IOwnProps {
    pinSidebar: boolean,
    expanded?: boolean,
    userCalendars: IUserCalendar[],
    userId: string,
    loading: boolean,
    retainSelection: boolean,
    initialView: string,
    initialState: () => void,
    calendarSelected: (option: IUserCalendar) => void,
    retainSelected: (e: React.ChangeEvent<HTMLInputElement>) => void
    viewSelected: (view: string) => void
}


export class SidebarMenu extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            expanded: this.props.expanded !== undefined ? this.props.expanded : false,
            selected: this.props.initialView
        };
    }


    public render() {
        return (
            this.props.pinSidebar ?
                <ClickOutside
                    onClickOutside={() => this.onSetSidebarOpen(false)} >
                    <SideNav
                        onToggle={(expanded: boolean) => this.onSetSidebarOpen(expanded)}
                        expanded={this.state.expanded} >
                        <SideNav.Toggle />
                        <SideNav.Nav selected={this.state.selected}>
                            <NavItem eventKey="reset" onSelect={() => this.props.initialState()}>
                                <NavIcon>
                                    <FaSyncAlt />
                                </NavIcon>
                                <NavText>
                                    Refresh
                                </NavText>
                            </NavItem>
                            <NavItem eventKey="dayGridMonth" onSelect={() => this.calendarViewChange("dayGridMonth")}>
                                <NavIcon>
                                    <FaCalendar />
                                </NavIcon>
                                <NavText>
                                    Month
                                </NavText>
                            </NavItem>
                            <NavItem eventKey="dayGridWeek" onSelect={() => this.calendarViewChange("dayGridWeek")}>
                                <NavIcon>
                                    <FaCalendarWeek />
                                </NavIcon>
                                <NavText>
                                    Week
                                </NavText>
                            </NavItem>
                            <NavItem eventKey="dayGrid" onSelect={() => this.calendarViewChange("dayGrid")}>
                                <NavIcon>
                                    <FaCalendarDay />
                                </NavIcon>
                                <NavText>
                                    Day
                                </NavText>
                            </NavItem>
                            <NavItem eventKey="listWeek" onSelect={() => this.calendarViewChange("listWeek")}>
                                <NavIcon>
                                    <FaCalendarAlt />
                                </NavIcon>
                                <NavText>
                                    List
                                </NavText>
                            </NavItem>
                            <NavItem eventKey="calendars" expanded={true}>
                                <NavIcon>
                                    <FaCalendarCheck />
                                </NavIcon>
                                <NavText>
                                    Calendars {this.props.loading ? <Load inlineDisplay={true} smallSize={true} /> : null}
                                </NavText>
                                {this.props.userCalendars.map(uc => 
                                    <NavItem key={uc.id} eventKey={uc.id} style={{color: '#222'}} >
                                        <NavText>
                                            <label><input disabled={this.props.loading} type="checkbox" name="id" checked={uc.selected} onChange={() => this.props.calendarSelected(uc)} /> {uc.name} {uc.invitee !== null ? <span className="badge badge-info text-small">shared by {uc.invitee}</span> : ""}</label>
                                        </NavText>
                                    </NavItem>
                                )}
                                <NavItem>
                                    <NavText>
                                        <div className="sidenav-divider" />
                                        <label><input disabled={this.props.loading} type="checkbox" onChange={(e) => this.props.retainSelected(e)} checked={this.props.retainSelection} /> Retain Selection</label>
                                    </NavText>
                                </NavItem>
                            </NavItem>
                        </SideNav.Nav>
                    </SideNav>
                </ClickOutside>
            : null
        );
    }

    private onSetSidebarOpen = (value: boolean) => {
        this.setState({ expanded: value })
    }

    private calendarViewChange = (view: string) => {
        this.setState({ expanded: false, selected: view })
        this.props.viewSelected(view)
    }

}