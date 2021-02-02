import IStoreState from '../../../state/IStoreState';
import { connect } from 'react-redux';
import SelectionRefinementForCategories, { IPropsFromState, IPropsFromDispatch } from '../category/SelectionRefinementForCategories';
import { CategoryType } from 'src/enums/CategoryType';
import { LoadCategoriesAction, OnChangeSelectedCategoryAction, OnChangeSelectedSecondCategoryAction } from 'src/state/contexts/common/Actions';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        categoryType: CategoryType.Spendings,
        categories: state.common.categories,
        secondCategories: state.common.secondCategories,
        secondTypeId: state.common.secondTypeId,
        selectedCat: state.common.selectedCat,
        selectedSecondCat: state.common.selectedSecondCat,
        loadingCategories: state.common.loadingCategories,
        loadingSecondCategories: state.common.loadingSecondCategories
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    loadCategories: LoadCategoriesAction.creator,
    onChangeSelectedCategory: OnChangeSelectedCategoryAction.creator,
    onChangeSelectedSecondCategory: OnChangeSelectedSecondCategoryAction.creator 
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(SelectionRefinementForCategories);


