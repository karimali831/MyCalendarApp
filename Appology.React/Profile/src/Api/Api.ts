import { rootUrl } from 'src/components/utils/Utils';
import { ICalendarSettings, IUser, IUserInfo } from 'src/models/IUser';
import { IUserType, IUserTypeDTO } from 'src/models/IUserType';
import { IUserTag } from 'src/models/IUserTag';
import { IGroup } from 'src/models/IGroup';

export class Api {
    public rootUrl: string = `${rootUrl}/api/profile`;

    public user = async (): Promise<IUserResponse> => {
        return fetch(`${this.rootUrl}/user`, {
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
        .then(data => data as IUserResponse);
    }

    public saveUserInfo = async (model: IUserInfo): Promise<boolean> => {
        return fetch(`${this.rootUrl}/saveuserinfo`, {
            method: "POST",
            body: JSON.stringify(model),
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin',
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as boolean);
    }

    public saveUserCalendarSettings = async (model: ICalendarSettings): Promise<boolean> => {
        return fetch(`${this.rootUrl}/savecalendarsettings`, {
            method: "POST",
            body: JSON.stringify(model),
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin',
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as boolean);
    }

    public saveUserType = async (model: IUserTypeDTO): Promise<ISaveTypeResponse> => {
        return fetch(`${this.rootUrl}/saveusertype`, {
            method: "POST",
            body: JSON.stringify(model),
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin',
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as ISaveTypeResponse);
    }

    public saveUserTags = async (tags: IUserTag[]): Promise<boolean> => {
        return fetch(`${this.rootUrl}/saveusertags`, {
            method: "POST",
            body: JSON.stringify(tags),
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin',
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as boolean);
    }

    public deleteUserType = async (id: number): Promise<IDeleteTypeResponse> => {
        return fetch(`${this.rootUrl}/deleteusertype/${id}`, {
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
        .then(data => data as IDeleteTypeResponse);
    }
}

export const api = new Api();

export interface IUserResponse {
    status: boolean
    user?: IUser,
    groups: IGroup[]
}

export interface IDeleteTypeResponse{
    status: boolean,
    message: string
}

export interface ISaveTypeResponse {
    type?: IUserType
}

