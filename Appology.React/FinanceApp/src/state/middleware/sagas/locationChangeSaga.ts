import { takeLatest, call, select, put } from "redux-saga/effects";
import { LOCATION_CHANGE } from "connected-react-router";
import * as Route from 'route-parser';
import { LoadSpendingsAction } from 'src/state/contexts/spending/Actions';
import { getLocation, getHash } from 'src/state/contexts/router/Selectors';
import { appPathUrl, rootUrl } from 'src/components/utils/Utils';
import { LoadIncomesAction } from 'src/state/contexts/income/Actions';
import { loadSpendingSummary } from './loadSpendingSummaryApiSaga';
import { spendingSummaryDateFilter, incomeSummaryDateFilter } from 'src/state/contexts/landing/Selectors';
import { loadIncomeSummary } from './loadIncomeSummaryApiSaga';
import { loadSpendings } from './loadSpendingsApiSaga';
import { ISpendingRequest, IIncomeRequest } from 'src/Api/Api';
import { IDateFilter } from 'src/models/IDateFilter';
import { DateFrequency } from 'src/enums/DateFrequency';
import { loadIncomes } from './loadIncomesApiSaga';
import { loadNotifications } from './loadNotificationsApiSaga';

interface IRoute {
    route: string,
    action: any
}

interface ISummaryListParam {
    catid: number,
    frequency: DateFrequency,
    interval: number,
    isfinance?: boolean,
    issecondcat?: boolean,
    fromdate?: string,
    todate?: string,
    categorytype: string
}

interface ISpendingSummaryParam {
    frequency: number,
    interval: number,
    fromDate: string,
    toDate: string
}


const routes: IRoute[] = [
    {
        route: `${appPathUrl}/home`,
        action: function* (params: ISpendingSummaryParam) {
            
            yield call(loadSpendingSummary, yield select(spendingSummaryDateFilter))
            yield call(loadIncomeSummary, yield select(incomeSummaryDateFilter))
            yield call(loadNotifications)
        }
    },
    {
        route: `${appPathUrl}/:categorytype*/:catId*/:frequency*/:interval*/:isFinance/:isSecondCat/:fromDate*/:toDate*`,
        action: function* (params: ISummaryListParam) {

            const dateFilter: IDateFilter = {
                frequency: params.frequency,
                interval: params.interval,
                fromDateRange: params.fromdate,
                toDateRange: params.todate
            }

            const spendingsRequest: ISpendingRequest = {
                catId: params.catid,
                dateFilter: dateFilter,
                isFinance: params.isfinance,
                isSecondCat: params.issecondcat
            }

            yield call(loadSpendings, spendingsRequest)
        }
    },
    {
        route: `${appPathUrl}/:categorytype*/:catId*/:frequency*/:interval*/:isSecondCat/:fromDate*/:toDate*`,
        action: function* (params: ISummaryListParam) {

            const dateFilter: IDateFilter = {
                frequency: params.frequency,
                interval: params.interval,
                fromDateRange: params.fromdate,
                toDateRange: params.todate
            }

            const incomesRequest: IIncomeRequest = {
                sourceId: params.catid,
                dateFilter: dateFilter,
                isSecondCat: params.issecondcat
            }

            yield call(loadIncomes, incomesRequest)
        }
    },
    {
        route: `${appPathUrl}/spending`,
        action: function* (params: ISummaryListParam) {
            yield put(new LoadSpendingsAction());
        }
    },
    {
        route: `${appPathUrl}/income`,
        action: function* (params: ISummaryListParam) {
            yield put(new LoadIncomesAction());
        }
    }
];

function* locationChangeSaga() {
    yield takeLatest(LOCATION_CHANGE, doCall);
}

export function* doCall() {
    let hash: string = yield select(getHash);
    if (hash && hash.startsWith('#')) {
        hash = hash.substring(1);
    }
    const location: string = yield select(getLocation);

    if (hash && hash !== "") {
        for (const r in routes) {
            const params = new Route(routes[r].route.toString().toLowerCase()).match(hash.toString().toLowerCase());
            if (params !== false) {
                window.location.href = hash;
                return;
            }
        }
    }

    for (const r in routes) {
        const route = rootUrl + routes[r].route.toLowerCase();
        const currentLocation = rootUrl + location.toLowerCase();
        const params = new Route((route)).match(currentLocation);

        if (params !== false) {
            yield call(routes[r].action, params);
            return;
        }
    }
}

export default locationChangeSaga;
