
export interface ICategory {
    id: number,
    name: string,
    typeId: number,
    secondTypeId: number,
    disabled: boolean,
    superCatId: number,
    monzoTag: string
}

export interface ICategoryDTO {
    name: string,
    typeId: number | null
}