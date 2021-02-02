export interface IBaseModel<T> {
    cat1: string,
    catId: number,
    secondCatId: number,
    isFinance: boolean,
    cat2: string,
    secondCats: T[],
    secondTypeId?: number,
    total: number,
    isSpecialCat: boolean
}