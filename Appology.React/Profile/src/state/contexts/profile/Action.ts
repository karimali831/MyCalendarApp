import { MenuTabs } from "src/Enums/MenuTabs";
import { IGroup } from "src/models/IGroup";
import { IUser } from "src/models/IUser";

// action types
export class ProfileActionTypes {
    public static readonly LoadUser = "@@profile/loaduser";
    public static readonly LoadUserSuccessAction = "@@profile/loadusersuccessaction";
    public static readonly LoadUserFailureAction = "@@profile/loaduserfailureaction";
    public static readonly ActiveMenuTabAction = "@@profile/activemenutabaction"
    public static readonly ConfirmAction = "@@profile/confirmaction"
}

export class LoadUser {
    public static readonly creator = () => new LoadUser();

    public readonly type = ProfileActionTypes.LoadUser;
}

export class ConfirmAction {
    public static readonly creator = (confirm: boolean) => new ConfirmAction(confirm);

    public readonly type = ProfileActionTypes.ConfirmAction

    constructor(
        public confirm: boolean
    ) { }
}

export class LoadUserSuccessAction {
    public static readonly creator = (groups: IGroup[], user: IUser | undefined) => new LoadUserSuccessAction(groups, user);

    public readonly type = ProfileActionTypes.LoadUserSuccessAction

    constructor(
        public groups: IGroup[],
        public user: IUser | undefined
    ) { }
}

export class LoadUserFailureAction {
    public static readonly creator = (errorMsg: string) => new LoadUserFailureAction(errorMsg);

    public readonly type = ProfileActionTypes.LoadUserFailureAction

    constructor(
        public errorMsg: string
    ) { }
}

export class ActiveMenuTabAction {
    public static readonly creator = (tab: MenuTabs) => new ActiveMenuTabAction(tab);

    public readonly type = ProfileActionTypes.ActiveMenuTabAction

    constructor(
        public tab: MenuTabs
    ) { }
}

// Create a discriminated union of all action types used to correctly type the
// actions in the reducer switch statement
export type ProfileActions =
    LoadUser |
    LoadUserSuccessAction |
    LoadUserFailureAction |
    ActiveMenuTabAction |
    ConfirmAction