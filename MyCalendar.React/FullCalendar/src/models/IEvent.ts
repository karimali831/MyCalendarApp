export interface IEvent {
    // standard props
    id: string,
    title: string,
    start: Date,
    end?: Date,
    startStr: string,
    endStr: string,
    allDay: boolean,
    url: string,
    classNames: string,
    editable: boolean,
    // backgroundColor: string, -- the dot
    eventBackgroundColor: string,
    textColor: string,
    display: string,
    // non standard props
    calendarId: string,
    userId: string,
    tagId: string,
    description: string,
    tentative: boolean,
    duration: string,
    eventUid: string,
    alarm: string,
    provider: string
}

export interface IEventDTO {
    // standard props
    id: string,
    title: string,
    startStr: string,
    endStr: string,
    allDay: boolean,
    // non standard props
    calendarId: string,
    tagId: string,
    description: string,
    tentative: boolean,
    eventUid: string,
    alarm: string,
}

export interface IEventDateSet {
    start: Date,
    end: Date,
    startStr: string,
    endStr: string,
    timeZone: string
}