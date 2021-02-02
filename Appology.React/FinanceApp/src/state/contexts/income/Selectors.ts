import { IIncomeRequest } from '../../../Api/Api';
import IStoreState from '../../../state/IStoreState';

export const getIncomeRequest = (state: IStoreState): IIncomeRequest => {
    return {
        sourceId: state.income.sourceId,
        dateFilter: state.spending.dateFilter
    };
}


