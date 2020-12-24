import { rootUrl } from 'src/components/utils/Utils';
import { IUserCalendar } from 'src/models/IUserCalendar';
import { IEvent } from 'src/models/IEvent';
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
}

export const api = new Api();

export interface IEventRequest {
    calendarIds: number[],
    year: number[],
    month: number[],
}

export interface IEventResponse {
    userId: string,
    events: IEvent[],
    userCalendars: IUserCalendar[],
    currentActivity: string[]
}

export interface IUserTagResponse {
    userTags: ITag[],
    cronofyReady: boolean
}