import { MenuTabs } from "src/Enums/MenuTabs";
import { IGroup } from "src/models/IGroup";
import { IUser } from "src/models/IUser";

export default interface IProfileState {
    user?: IUser,
    groups: IGroup[],
    loading: boolean,
    confirmAction: boolean,
    activeMenuTab: MenuTabs
}

export class ProfileState {
    public static readonly intialState = {
        user: undefined,
        loading: false,
        activeMenuTab: MenuTabs.ProfileInfo,
        groups: [],
        confirmAction: false
    }
}