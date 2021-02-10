import IBaseModel from "@appology/react-components/src/SelectionRefinement/IBaseModel";
import { IUserTag } from "./IUserTag";
import { IUserType } from "./IUserType";

export interface IUser {
    userInfo: IUserInfo,
    avatar: string,
    userBuddys: IUserBuddy[],
    userTags: IUserTag[],
    userCalendars: IUserType[],
    userTypes: IUserType[],
    calendarSettings: ICalendarSettings
}

export interface IUserInfo {
    userID: string,
    name: string,
    password: string,
    email: string,
    phoneNumber: string
}

export interface IUserBuddy {
    userID: string,
    name: string
}

export interface ICalendarSettings {
    enableCronofy: boolean,
    defaultCalendarView: string,
    defaultNativeCalendarView: string,
    selectedCalendars: string
}

export interface IModel extends IBaseModel {
    id: string,
    name: string
}

