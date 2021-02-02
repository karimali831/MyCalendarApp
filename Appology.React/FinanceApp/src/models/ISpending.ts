export interface ISpending {
    id: number,
    name: string,
    amount: number,
    date: Date,
    info: string,
    category: string,
    secondCategory: string,
    cashExpense: boolean
}

export interface ISpendingDTO {
    name: string,
    date: string,
    amount: number,
    catId?: number | null,
    secondCatId?: number | null,
    financeId?: number,
    cashExpense: boolean
}

export interface ISpendingSummary
{
    cat1: string,
    catId: number,
    secondCatId: number,
    isFinance: boolean,
    cat2: string,
    secondCats: ISpendingSummary[],
    total: number,
    average: string,
    superCatId1: number,
    superCatId2: number,
    isSpecialCat: boolean
}