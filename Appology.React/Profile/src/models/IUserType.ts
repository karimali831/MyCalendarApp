import { TypeGroup } from "src/Enums/TypeGroup";

export interface IUserType {
    id: number,
    name: string,
    userCreatedId: string,
    invitee: string,
    selected: boolean,
    inviteeIdsList: string[],
    groupId: TypeGroup
}

export interface IUserTypeDTO {
    id?: number,
    name: string,
    inviteeIds: string,
    groupId: TypeGroup
}