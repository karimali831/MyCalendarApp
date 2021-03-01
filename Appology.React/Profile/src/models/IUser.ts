import IUserBuddy from "@appology/react-components/dist/UserTypes/IUserBuddy";
import IUserType from "@appology/react-components/dist/UserTypes/IUserType";
import IBaseModel from "@appology/react-components/dist/SelectionRefinement/IBaseModel";
import { IUserTag } from "./IUserTag";

export interface IUser {
    userInfo: IUserInfo,
    avatar: string,
    userBuddys: IUserBuddy[],
    userTags: IUserTag[],
    userCalendars: IUserType[],
    userTypes: IUserType[],
    inviterShareLink: string,
    calendarSettings: ICalendarSettings
}

export interface IUserInfo {
    userID: string,
    name: string,
    password: string,
    email: string,
    phoneNumber: string
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

