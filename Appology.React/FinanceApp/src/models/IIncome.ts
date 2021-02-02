export interface IIncome {
    id: number,
    name: string,
    source: string,
    secondSource: string,
    date: Date,
    amount: number,
    weekNo: number
}

export interface IIncomeDTO {
    name: string,
    sourceId: number,
    secondSourceId: number | undefined,
    date: string,
    amount: number
}

export interface IIncomeSummary {
    cat1: string,
    catId: number,
    secondCatId: number,
    isFinance: boolean, // remove this
    cat2: string,
    secondCats: IIncomeSummary[],
    total: number,
    average: string,
    isSpecialCat: boolean
}