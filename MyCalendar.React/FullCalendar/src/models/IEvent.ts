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
    // non standard props
    calendarId: number,
    userId: string,
    tagId: string,
    description: string,
    tentative: boolean,
    duration: string,
    eventUid?: string | null,
    alarm: string,
    provider: string,
    reminder: boolean,
    avatar: string
}

export interface IEventDTO {
    // standard props
    id: string,
    title: string,
    startStr: string,
    endStr: string,
    allDay: boolean,
    // non standard props
    calendarId: number,
    reminder: boolean,
    tagId: string,
    description: string,
    tentative: boolean,
    eventUid?: string | null,
    alarm: string,
    // no transfer props
    duration?: string,
    userId?: string
}

export interface IEventSelect {
    dateStart?: Date,
    dateEnd?: Date,
    event?: IEventDTO,
    eventEdit?: IEventDTO
}

export interface IEventDateSet {
    start: Date,
    end: Date,
    startStr: string,
    endStr: string,
    timeZone: string
}