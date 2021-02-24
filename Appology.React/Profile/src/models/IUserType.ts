import { TypeGroup } from "src/Enums/TypeGroup";

export interface IUserType {
    key: number,
    title: string,
    invitee: string,
    selected: boolean,
    inviteeIdsList: string[],
    groupId: TypeGroup,
    superTypeId?: number,
    isLeaf: boolean,
    children: IUserType[]
}

export interface IUserTypeDTO {
    id?: number,
    name: string,
    inviteeIds: string,
    groupId: TypeGroup,
    superTypeId?: number
}