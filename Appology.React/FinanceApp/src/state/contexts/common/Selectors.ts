
import IStoreState from '../../../state/IStoreState';
import { CategoryType } from 'src/enums/CategoryType';

export const getSecondTypeId = (state: IStoreState): number | undefined =>
    state.common.secondTypeId

export const getCategoryType = (state: IStoreState): CategoryType | undefined =>
    state.common.categoryType
