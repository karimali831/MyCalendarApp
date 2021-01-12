import { rootUrl } from 'src/components/utils/Utils';
import { IUserCalendar } from 'src/models/IUserCalendar';
import { IEvent, IEventDTO } from 'src/models/IEvent';
import { ITag } from 'src/models/ITag';

export class Api {
    public rootUrl: string = `${rootUrl}/api/calendar`;

    public events = async (request: IEventRequest): Promise<IEventResponse> => {
        return fetch(`${this.rootUrl}/events`, {
            method: "POST",
            body: JSON.stringify(request),
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin'
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as IEventResponse);
    }

    public userTags = async (): Promise<IUserTagResponse> => {
        return fetch(`${this.rootUrl}/usertags`, {
            method: "GET",
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin'
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as IUserTagResponse);
    }

    public retainSelection = async (calendarIds: number[] | null): Promise<boolean> => {
        return fetch(`${this.rootUrl}/retainselection`, {
            method: "POST",
            body: JSON.stringify(calendarIds),
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin'
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as boolean);
    }

    public saveEvent = async (event: IEventDTO): Promise<IEvent> => {
        return fetch(`${this.rootUrl}/save`, {
            method: "POST",
            body: JSON.stringify(event),
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin'
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as IEvent);
    }

    public alarmInfo = async (tagId: string): Promise<string> => {
        return fetch(`${this.rootUrl}/alarminfo/${tagId}`, {
            method: "GET",
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin'
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as string);
    }

    public deleteEvent = async (eventId: string): Promise<boolean> => {
        return fetch(`${this.rootUrl}/delete/${eventId}`, {
            method: "GET",
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin'
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as boolean);
    }
}

export const api = new Api();

export interface IEventRequest {
    calendarIds: number[],
    year: number[],
    month: number[]
}

export interface IEventResponse {
    userId: string,
    events: IEvent[],
    userCalendars: IUserCalendar[],
    retainSelection: boolean,
    retainView: string
}

export interface IUserTagResponse {
    userTags: ITag[],
    cronofyReady: boolean
}