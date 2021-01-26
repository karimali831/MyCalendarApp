
import { call, put, select, takeLatest } from 'redux-saga/effects';
import { OrderActionTypes, OrderOverviewAction, SaveOrderFailureAction, SaveOrderSuccessAction, UpdateOrderAction } from 'src/state/contexts/order/Actions';
import { getOrderOverview, getOrderForm, getOrder, getTrip } from 'src/state/contexts/order/Selectors';
import { IOrder, IOrderForm, IOrderOverview } from 'src/models/IOrder';
import { ToggleDriverStep4Action, LandingActionTypes, SetActiveStepAction, UpdateTripAction, SelectedOrderEnableStepsAction, SelectedServiceAction, DistanceMatrixAction } from 'src/state/contexts/landing/Actions';
import { IDefaultConfig } from 'src/models/IDefaultConfig';
import { getConfig, getPickupGeometry, getSelectedCustomer, getSelectedDriver, getSelectedService, getTripOverview } from 'src/state/contexts/landing/Selectors';
import { ITrip, ITripOverview } from 'src/models/ITrip';
import { IStakeholder } from 'src/models/IStakeholder';
import IBaseModel from '@appology/react-components/src/SelectionRefinement/IBaseModel';
import { api, ISaveOrderRequest, ISaveOrderResponse } from 'src/Api/Api';
import { ReportErrorAction } from 'src/state/contexts/error/Actions';
import { IGoogleGeoLocation } from 'src/Api/GoogleApi';
import { IGoogleAutoCompleteSearch } from 'src/models/IGoogleAutoComplete';

export default function* orderApiSaga() {
    yield takeLatest(OrderActionTypes.UpdateOrder, updateOrderChange);
    yield takeLatest(LandingActionTypes.UpdateTrip, updateOrderChange);
    yield takeLatest(LandingActionTypes.UpdateConfig, updateOrderChange);
    yield takeLatest(OrderActionTypes.SetDriverStep4, setDriverStep4);
    yield takeLatest(OrderActionTypes.SaveOrder, saveOrder);
    yield takeLatest(OrderActionTypes.SelectedOrder, selectedOrder)
}

export function * setDriverStep4() {
    yield put(new SetActiveStepAction(3));
}

export function* updateOrderChange() {
    const order : IOrderForm = yield select(getOrderForm)
    const config : IDefaultConfig = yield select(getConfig)
    const orderOverview: IOrderOverview = yield select(getOrderOverview);

    yield put(new OrderOverviewAction(orderOverview));  
    yield put(new ToggleDriverStep4Action(order.orderValue >= config.minimumOrderValue))
}

export function* selectedOrder() {
    const order : IOrder = yield select(getOrder)
    const trip: ITrip = yield select(getTrip);
    const customer : IStakeholder = yield select(getSelectedCustomer);

    if (order !== undefined && trip !== undefined && customer.apiLat !== undefined && customer.apiLng !== undefined) {

        const orderForm : IOrderForm = {
            orderId: order.orderId,
            items: JSON.parse(order.items),
            orderValue: order.orderValue,
            orderFee: order.orderFee,
            totalItems: order.totalItems,
        }

        const tripOverview : ITripOverview = {
            tripId: trip.tripId,
            pickupPlace: trip.pickupPlace,
            pickupId: trip.pickupId,
            distance: trip.distance,
            duration: trip.duration,
            dropOffAddress: trip.dropOffAddress,
            dropOffPostCode: trip.dropOffAddress,
            stakeholderLocation: {
                lat: customer.apiLat,
                lng: customer.apiLng
            }
        }

        const selectedService : IBaseModel = { id: order.serviceId.toString(), name: order.serviceName ?? "" }
        const store: IGoogleAutoCompleteSearch = {id: trip.pickupId, name: trip.pickupPlace }
        const storeLocation: IGoogleGeoLocation = { lat: trip.pickupLat, lng: trip.pickupLng }

        yield put(new SelectedServiceAction(selectedService))
        yield put(new UpdateOrderAction(orderForm))
        yield put(new UpdateTripAction(tripOverview))
        yield put(new DistanceMatrixAction(store, storeLocation, tripOverview.stakeholderLocation))
        yield put(new SelectedOrderEnableStepsAction())
    }
}

export function* saveOrder() {

    const orderForm : IOrderForm = yield select(getOrderForm)
    const orderOverview: IOrderOverview = yield select(getOrderOverview);
    const customer : IStakeholder = yield select(getSelectedCustomer);
    const service : IBaseModel = yield select(getSelectedService);

    const order : IOrder = {
        orderId: orderForm.orderId,
        customerId: customer.id,
        serviceId: Number(service.id),
        items: JSON.stringify(orderForm.items),
        orderValue: orderForm.orderValue,
        serviceFee: orderOverview.serviceFee,
        orderFee: orderForm.orderFee,
        deliveryFee: orderOverview.deliveryFee,
        totalItems: orderForm.totalItems,
        invoice: orderOverview.invoiceAmt,
        net: orderOverview.netProfit,
        driverFee: orderOverview.driverFee,
        driverEarning: orderOverview.netProfit - orderOverview.driverFee
    }

    const driver : IBaseModel = yield select(getSelectedDriver);
    const tripOverview : ITripOverview = yield select(getTripOverview);
    const pickupLocation : IGoogleGeoLocation = yield select(getPickupGeometry)

    const trip : ITrip = {
        tripId: tripOverview.tripId,
        assignedRunnerId: driver.id,
        pickupId: tripOverview.pickupId,
        pickupPlace: tripOverview.pickupPlace,
        pickupLat: pickupLocation.lat,
        pickupLng: pickupLocation.lng,
        dropOffAddress: tripOverview.dropOffAddress,
        dropOffPostCode: tripOverview.dropOffPostCode,
        distance: tripOverview.distance,
        duration: tripOverview.duration

    }

    const request : ISaveOrderRequest = { order: order, trip: trip }

    try {

        // Start the API call asynchronously
        const orderResult: ISaveOrderResponse = yield call(api.saveOrder, request);

        // Create an action to dispatch on success with the returned entity from API
        const orderResultAction = new SaveOrderSuccessAction(orderResult.order, orderResult.trip);

        // Dispatch the new action with Redux
        yield put(orderResultAction);


    } catch (e) {

        // Dispatch a failure action to Redux
        yield put(new SaveOrderFailureAction(e.message));
        yield put(new ReportErrorAction(e.message));
        return;
    }

}