import { IDateFilter } from 'src/models/IDateFilter';
import { CategoryType } from 'src/enums/CategoryType';
import { ICategory } from 'src/models/ICategory';
import { DataType } from 'src/enums/DataType';

// action types
export class CommonActionTypes {

    public static readonly DateFilterChange = "@@common/datefilterchange"
    public static readonly DateFilterChangeSuccess = "@@common/datefilterchangesuccess"
    public static readonly LoadCategories = "@@common/loadcategories"
    public static readonly LoadCategoriesSuccess = "@@common/loadcategoriessuccess"
    public static readonly LoadCategoriesFailure = "@@common/loadcategoriesfailure"
    public static readonly LoadSecondCategoriesSuccess = "@@common/loadsecondcategoriessuccess"
    public static readonly LoadSecondCategoriesFailure = "@@common/loadsecondcategoriesfailure"
    public static readonly OnChangeSelectedCategory = "@@common/onchangeselectedcategory" // load second categories 
    public static readonly OnChangeSelectedSecondCategory = "@@common/onchangeselectedsecondcategory"
}

// action defined as classes with a creator as a static function
export class DateFilterChangeAction {
    public static readonly creator = (filter: IDateFilter, dataType: DataType) => new DateFilterChangeAction(filter, dataType);

    public readonly type = CommonActionTypes.DateFilterChange

    constructor(
        public filter: IDateFilter,
        public dataType: DataType
    ) { }
}

export class DateFilterChangeSuccessAction {
    public static readonly creator = () => new DateFilterChangeSuccessAction();

    public readonly type = CommonActionTypes.DateFilterChangeSuccess;

}

export class LoadCategoriesAction {
    public static readonly creator = (categoryType: CategoryType) => new LoadCategoriesAction(categoryType);

    public readonly type = CommonActionTypes.LoadCategories;

    constructor(
        public categoryType: CategoryType
    ) { }
}

export class LoadCategoriesSuccessAction {
    public static readonly creator = (categories: ICategory[]) => new LoadCategoriesSuccessAction(categories);

    public readonly type = CommonActionTypes.LoadCategoriesSuccess

    constructor(
        public categories: ICategory[]
    ) { }
}

export class LoadSecondCategoriesSuccessAction {
    public static readonly creator = (categories: ICategory[]) => new LoadSecondCategoriesSuccessAction(categories);

    public readonly type = CommonActionTypes.LoadSecondCategoriesSuccess

    constructor(
        public categories: ICategory[]
    ) { }
}

export class LoadCategoriesFailureAction {
    public static readonly creator = (errorMsg: string) => new LoadCategoriesFailureAction(errorMsg);

    public readonly type = CommonActionTypes.LoadCategoriesFailure;

    constructor(
        public errorMsg: string
    ) { }
}

export class LoadSecondCategoriesFailureAction {
    public static readonly creator = (errorMsg: string) => new LoadSecondCategoriesFailureAction(errorMsg);

    public readonly type = CommonActionTypes.LoadSecondCategoriesFailure;

    constructor(
        public errorMsg: string
    ) { }
}

export class OnChangeSelectedCategoryAction {
    public static readonly creator = (selectedCat: number, secondTypeId: number) => new OnChangeSelectedCategoryAction(selectedCat, secondTypeId);

    public readonly type = CommonActionTypes.OnChangeSelectedCategory;

    constructor(
        public selectedCat: number,
        public secondTypeId: number
    ) { }
}

export class OnChangeSelectedSecondCategoryAction {
    public static readonly creator = (selectedSecondCat: number) => new OnChangeSelectedSecondCategoryAction(selectedSecondCat);

    public readonly type = CommonActionTypes.OnChangeSelectedSecondCategory;

    constructor(
        public selectedSecondCat: number
    ) { }
}

// Create a discriminated union of all action typed used to correctly type the
// actions in the reducer switch statement
export type CommonActions =
    DateFilterChangeAction |
    DateFilterChangeSuccessAction |
    LoadCategoriesAction |
    LoadCategoriesSuccessAction | 
    LoadSecondCategoriesSuccessAction |
    LoadCategoriesFailureAction |
    LoadSecondCategoriesFailureAction |
    OnChangeSelectedCategoryAction |
    OnChangeSelectedSecondCategoryAction