import { IDateFilter } from 'src/models/IDateFilter';
import { ICategory } from 'src/models/ICategory';
import { CategoryType } from 'src/enums/CategoryType';
import { DataType } from 'src/enums/DataType';

export default interface ICommonState {
    dateFilter?: IDateFilter,
    dataType?: DataType,
    categoryType?: CategoryType,
    categories: ICategory[],
    secondCategories: ICategory[],
    secondTypeId?: number,
    selectedCat?: number,
    selectedSecondCat?: number,
    loadingCategories: boolean,
    loadingSecondCategories: boolean
}

export class CommonState {
    public static readonly intialState = {
        dateFilter: undefined,
        dataType: undefined,
        categoryType: undefined,
        categories: [],
        secondCategories: [],
        secondTypeId: undefined,
        selectedCat: undefined,
        selectedSecondCat: undefined,
        loadingCategories: false,
        loadingSecondCategories: false
    }
}