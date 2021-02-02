import { CommonActions, CommonActionTypes } from "./Actions";
import ICommonState, { CommonState } from './ICommonState';
import { Reducer } from 'redux';

const CommonReducer: Reducer<ICommonState, CommonActions> =
    (state = CommonState.intialState, action) => {
        switch (action.type) {
            case CommonActionTypes.DateFilterChange:
                return { ...state, 
                    ...{ 
                        dateFilter: action.filter,
                        dataType: action.dataType
                    } 
                };

            case CommonActionTypes.LoadCategories:
                    return { ...state, 
                        ...{
                            loadingCategories: true, 
                            categoryType: action.categoryType 
                        }
                    };

            case CommonActionTypes.LoadCategoriesSuccess:
                return { ...state, 
                    ...{ 
                        categories: action.categories,
                        loadingCategories: false
                    } 
                };

            case CommonActionTypes.LoadSecondCategoriesSuccess:
                return { ...state, 
                    ...{ 
                        secondCategories: action.categories,
                        loadingSecondCategories: false
                    } 
                };

            case CommonActionTypes.OnChangeSelectedCategory:
                    return { ...state, 
                        ...{ 
                            loadingSecondCategories: true,
                            selectedCat: action.selectedCat,
                            secondTypeId: action.secondTypeId ? action.secondTypeId : undefined,
                            selectedSecondCat: undefined
                        } 
                    };

            case CommonActionTypes.OnChangeSelectedSecondCategory:
                    return { ...state, 
                        ...{ 
                            selectedSecondCat: action.selectedSecondCat
                        } 
                    };


            default:
                return state;
        }
    }

export default CommonReducer;