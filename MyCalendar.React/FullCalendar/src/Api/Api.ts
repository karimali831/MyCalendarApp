import { rootUrl } from 'src/components/utils/Utils';
import { IUserCalendar } from 'src/models/IUserCalendar';
import { IEvent } from 'src/models/IEvent';

export class Api {
    public rootUrl: string = `${rootUrl}/api/calendar`;

    public events = async (calendarIds: number[]): Promise<IEventResponse> => {
        return fetch(`${this.rootUrl}/events`, {
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
        .then(data => data as IEventResponse);
    }
}

export const api = new Api();

export interface IEventResponse {
    userId: string,
    events: IEvent[],
    userCalendars: IUserCalendar[],
    currentActivity: string[]
}