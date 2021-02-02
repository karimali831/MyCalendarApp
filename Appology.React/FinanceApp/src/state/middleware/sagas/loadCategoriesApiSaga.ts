import { ICategoryResponse } from '../../../Api/Api'
import { call, put, takeLatest, select } from 'redux-saga/effects';
import { ReportErrorAction } from '../../contexts/error/Actions';
import { CommonActionTypes, LoadCategoriesSuccessAction, LoadCategoriesFailureAction, LoadSecondCategoriesSuccessAction} from 'src/state/contexts/common/Actions';
import { commonApi } from 'src/Api/CommonApi';
import { CategoryType } from 'src/enums/CategoryType';
import { getSecondTypeId, getCategoryType } from 'src/state/contexts/common/Selectors';

export default function* loadCategoriesApiSaga() {
    yield takeLatest(CommonActionTypes.LoadCategories, loadCategories, false);
    yield takeLatest(CommonActionTypes.OnChangeSelectedCategory, loadCategories, true)
}

export function* loadCategories(subCategories: boolean) {
    try {

        const secondTypeId: number = yield select(getSecondTypeId);
        const categoryType: CategoryType = yield select(getCategoryType);

        const categoryTypeId = (secondTypeId !== undefined && subCategories === true) ? secondTypeId : categoryType;
    
        // Start the API call asynchronously
        const result: ICategoryResponse = yield call(commonApi.categories, categoryTypeId);

        // Create an action to dispatch on success with the returned entity from API
        const resultAction = subCategories ? 
            new LoadSecondCategoriesSuccessAction(result.categories) : 
            new LoadCategoriesSuccessAction(result.categories);

        // Dispatch the new action with Redux
        yield put(resultAction);
        
    } catch (e) {

        // Dispatch a failure action to Redux
        yield put(new LoadCategoriesFailureAction(e.message));
        yield put(new ReportErrorAction(e.message));
        return;
    }
}