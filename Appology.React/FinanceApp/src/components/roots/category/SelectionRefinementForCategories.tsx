import * as React from 'react'
import { CategoryType } from 'src/enums/CategoryType';
import { LoadCategoriesAction, OnChangeSelectedCategoryAction, OnChangeSelectedSecondCategoryAction, } from 'src/state/contexts/common/Actions';
import { ICategory } from 'src/models/ICategory';
import SelectCategories from './SelectCategories';

export interface IPropsFromState {
    categoryType?: CategoryType,
    categories: ICategory[],
    secondCategories: ICategory[],
    secondTypeId?: number,
    selectedCat?: number,
    selectedSecondCat?: number,
    loadingCategories: boolean,
    loadingSecondCategories: boolean,
    showAllSubcats?: boolean | false
}

export interface IPropsFromDispatch {
    loadCategories: (categoryType: CategoryType) => LoadCategoriesAction
    onChangeSelectedCategory: (selectedCat: number, secondTypeId?: number) => OnChangeSelectedCategoryAction,
    onChangeSelectedSecondCategory: (selectedSecondCat: number) => OnChangeSelectedSecondCategoryAction
}

type AllProps = IPropsFromState & IPropsFromDispatch

const SelectCategoriesForSummary: React.SFC<AllProps> = (props) =>
    <SelectCategories
        categories={props.categories}
        secondCategories={props.secondCategories}
        secondTypeId={props.secondTypeId}
        loadingCategories={props.loadingCategories}
        loadingSecondCategories={props.loadingSecondCategories}
        categoryType={props.categoryType}
        loadCategories={props.loadCategories}
        onChangeSelectedCategory={props.onChangeSelectedCategory}
        onChangeSelectedSecondCategory={props.onChangeSelectedSecondCategory}
        selectedCat={props.selectedCat}
        selectedSecondCat={props.selectedSecondCat}
        showAllSubcats={props.showAllSubcats}
    />

export default SelectCategoriesForSummary;