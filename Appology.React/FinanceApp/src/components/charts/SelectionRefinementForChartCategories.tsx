
import { connect } from 'react-redux';
import { LoadCategoriesAction, OnChangeSelectedCategoryAction, OnChangeSelectedSecondCategoryAction } from 'src/state/contexts/common/Actions';
import IStoreState from 'src/state/IStoreState';
import SelectionRefinementForCategories, { IPropsFromState, IPropsFromDispatch } from '../roots/category/SelectionRefinementForCategories';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        categoryType: state.chart.request !== undefined ? state.chart.request.categoryType : state.common.categoryType,
        categories: state.common.categories,
        secondCategories: state.common.secondCategories,
        secondTypeId: state.common.secondTypeId,
        selectedCat: state.chart.request !== undefined ? state.chart.request.catId : state.common.selectedCat,
        selectedSecondCat: state.chart.request !== undefined ? state.chart.request.secondCatId : state.common.selectedSecondCat, 
        loadingCategories: state.common.loadingCategories,
        loadingSecondCategories: state.common.loadingSecondCategories,
        showAllSubcats: true
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


