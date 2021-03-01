import { rootUrl } from 'src/components/utils/Utils';
import { ICalendarSettings, IUser, IUserInfo } from 'src/models/IUser';
import { IUserTag } from 'src/models/IUserTag';
import { Variant, TypeGroup } from '@appology/react-components';
import IUserTypeDTO from '@appology/react-components/dist/UserTypes/IUserTypeDTO'
import IGroup from '@appology/react-components/dist/UserTypes/IGroup';
import IUserType from '@appology/react-components/dist/UserTypes/IUserType';

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

    public saveUserType = async (model: IUserTypeDTO): Promise<ITypeChangeResponse> => {
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
        .then(data => data as ITypeChangeResponse);
    }

    public moveUserType = async (id: number, groupId: TypeGroup, superTypeId?: number): Promise<ITypeChangeResponse> => {
        return fetch(`${this.rootUrl}/moveusertype/${id}/${groupId}/${superTypeId}`, {
            method: "GET",
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
        .then(data => data as ITypeChangeResponse);
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

    public deleteUserType = async (id: number, groupId: TypeGroup): Promise<ITypeChangeResponse> => {
        return fetch(`${this.rootUrl}/deleteusertype/${id}/${groupId}`, {
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
        .then(data => data as ITypeChangeResponse);
    }

    public removeBuddy = async (buddyId: string, existConfirm: boolean): Promise<IRemoveBuddyResponse> => {
        return fetch(`${this.rootUrl}/removeuserbuddy/${buddyId}/${existConfirm}`, {
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
        .then(data => data as IRemoveBuddyResponse);
    }
}

export const api = new Api();

export interface IUserResponse {
    status: boolean
    user?: IUser,
    groups: IGroup[]
}


export interface ITypeChangeResponse {
    status: boolean,
    responseMsg: string,
    userTypes: IUserType[]
}

export interface IRemoveBuddyResponse {
    responseVariant: Variant,
    responseMsg: string
}