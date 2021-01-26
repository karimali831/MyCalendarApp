import { takeLatest, call, select } from "redux-saga/effects";
import { LOCATION_CHANGE } from "connected-react-router";
import * as Route from 'route-parser';
import { getLocation, getHash } from 'src/state/contexts/router/Selectors';
import { loadStakeholders } from './loadStakeholdersApiSaga';
import { rootUrl } from "src/components/utils/Utils";
import { getStakeholderId, getStakeholderSearchFilter } from "src/state/contexts/landing/Selectors";

interface IRoute {
    route: string,
    action: any
}

interface IIdParam {
    id: string,
    secondid: string,
}

const routes: IRoute[] = [
    {
        route: '/home',
        action: function* (params: IIdParam) {
            yield call(loadStakeholders, yield select(getStakeholderSearchFilter), yield select(getStakeholderId))
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
