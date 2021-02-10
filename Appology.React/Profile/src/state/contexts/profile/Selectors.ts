import { IUser } from "src/models/IUser";
import IStoreState from "src/state/IStoreState";

export const getUser = (state: IStoreState): IUser | undefined => {
    return state.profile.user 
}

