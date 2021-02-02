import { ISpendingRequest } from '../../../Api/Api';
import IStoreState from '../../../state/IStoreState';

export const getSpendingsRequest = (state: IStoreState): ISpendingRequest => {
    return {
        catId: state.spending.catId,
        dateFilter: state.spending.dateFilter,
        isFinance: state.spending.isFinance,
        isSecondCat: state.spending.isSecondCat
    };
}


