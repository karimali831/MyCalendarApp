
import { call, put, select, takeLatest } from 'redux-saga/effects';
import { ReportErrorAction } from '../../contexts/error/Actions';
import { LandingActionTypes, DistanceMatrixSuccessAction, DistanceMatrixFailureAction, UpdateTripAction } from '../../contexts/landing/Actions';
import { getPickupLocation, getSelectedCustomer, getStakeholderLocation, getTripOverview } from 'src/state/contexts/landing/Selectors';
import { IGoogleAutoCompleteSearch } from 'src/models/IGoogleAutoComplete';
import { googleApi, IGoogleDistanceMatrixResponse, IGoogleGeoLocation } from 'src/Api/GoogleApi';
import { ITripOverview } from 'src/models/ITrip';
import { IStakeholder } from 'src/models/IStakeholder';

export default function* tripApiSaga() {
    yield takeLatest(LandingActionTypes.DistanceMatrix, distanceMatrixRequest);
}

export function* distanceMatrixRequest() {
    const pickupPlace: IGoogleAutoCompleteSearch = yield select(getPickupLocation);
    const stakeholderLocation: IGoogleGeoLocation = yield select(getStakeholderLocation)

    yield call(distanceMatrix, pickupPlace, stakeholderLocation);
}

export function* distanceMatrix(store: IGoogleAutoCompleteSearch, stakeholderLocation: IGoogleGeoLocation) {
    try {

        // Start the API call asynchronously
        const result: IGoogleDistanceMatrixResponse = yield call(googleApi.distanceMatrix, store.id, stakeholderLocation);

        if (result.rows.length === 1 && result.rows[0].elements.length === 1) {

            // Update trip
            const matrix = result.rows[0].elements[0];
            const selectedCustomer : IStakeholder = yield select(getSelectedCustomer);
            const trip : ITripOverview | undefined = yield select(getTripOverview);

            const tripOverview: ITripOverview = {
                tripId: trip?.tripId ?? "",
                pickupPlace: store.name,
                pickupId: store.id,
                distance: matrix.distance.text,
                duration: matrix.duration.text,
                stakeholderLocation: stakeholderLocation,
                dropOffAddress: selectedCustomer.address1,
                dropOffPostCode: selectedCustomer.postcode,
            }
            
            yield put(new UpdateTripAction(tripOverview))

            // Create an action to dispatch on success with the returned entity from API
            const resultAction = new DistanceMatrixSuccessAction(result.rows);

            // Dispatch the new action with Redux
            yield put(resultAction);
        }
        
    } catch (e) {

        // Dispatch a failure action to Redux
        yield put(new DistanceMatrixFailureAction(e.message));
        yield put(new ReportErrorAction(e.message));
        return;
    }
}