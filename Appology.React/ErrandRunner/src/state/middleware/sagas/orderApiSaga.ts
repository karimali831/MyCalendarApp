
import { call, put, select, takeLatest } from 'redux-saga/effects';
import { DispatchOrderAction, OrderActionTypes, OrderOverviewAction, OrderPaidAction, OrderStatusAction, ResetOrderAction, SaveOrderFailureAction, SaveOrderSuccessAction, SaveStatusAction, SetDeliveryDateAction, UpdateOrderAction } from 'src/state/contexts/order/Actions';
import { getOrderOverview, getOrderForm, getOrder, getTrip, saveOrderStatus, getOrderDispatch, getOrderPaid, getDeliveryDate, getDeliveryTimeslot, getStripePaymentConfirmationId } from 'src/state/contexts/order/Selectors';
import { IOrder, IOrderForm, IOrderOverview } from 'src/models/IOrder';
import { LandingActionTypes, UpdateTripAction, SelectedOrderEnableStepsAction, SelectedServiceAction, DistanceMatrixAction, ToggleDriverStep4Action } from 'src/state/contexts/landing/Actions';
import { getConfig, getPickupGeometry, getSelectedCustomer, getSelectedDriver, getSelectedService, getTripOverview } from 'src/state/contexts/landing/Selectors';
import { ITrip, ITripOverview } from 'src/models/ITrip';
import { IStakeholder } from 'src/models/IStakeholder';
import IBaseModel from '@appology/react-components/src/SelectionRefinement/IBaseModel';
import { api, ISaveOrderRequest, ISaveOrderResponse } from 'src/Api/Api';
import { ReportErrorAction } from 'src/state/contexts/error/Actions';
import { IGoogleGeoLocation } from 'src/Api/GoogleApi';
import { IGoogleAutoCompleteSearch } from 'src/models/IGoogleAutoComplete';
import { OrderAction } from 'src/models/IDispatchStatus';
import { SaveStatus } from 'src/Enums/SaveStatus';
import { IDefaultConfig } from 'src/models/IDefaultConfig';
import { Variant } from '@appology/react-components';

export default function* orderApiSaga() {
    yield takeLatest(OrderActionTypes.UpdateOrder, updateOrderChange);
    yield takeLatest(LandingActionTypes.UpdateTrip, updateOrderChange);
    yield takeLatest(LandingActionTypes.UpdateConfig, updateOrderChange);
    yield takeLatest(OrderActionTypes.SaveOrder, saveOrder);
    yield takeLatest(OrderActionTypes.SelectedOrder, selectedOrder);
    yield takeLatest(OrderActionTypes.OrderPaid, setOrderPaid)
    yield takeLatest(OrderActionTypes.DispatchOrder, setOrderDispatch)

    // reset existing order if ResetOrder is yielded
    yield takeLatest(LandingActionTypes.ResetOrder, resetOrder)
}


// When LandingActionType ResetOrder is called then yield OrderActionType ResetOrder
export function* resetOrder() {
    yield put(new ResetOrderAction());
}

export function* updateOrderChange() {
    const order : IOrderForm = yield select(getOrderForm)
    const config : IDefaultConfig = yield select(getConfig)
    const orderOverview: IOrderOverview = yield select(getOrderOverview);
    
    yield put(new ToggleDriverStep4Action(order.orderValue >= config.minimumOrderValue))
    yield put(new OrderOverviewAction(orderOverview));  
}

export function* setOrderPaid() {
    const order : IOrder = yield select(getOrder);
    const paid : boolean = yield select(getOrderPaid);
    const stripePaymentConfirmationId : string | undefined = yield select(getStripePaymentConfirmationId);

    const orderPaid: boolean = yield call(api.setOrderPaid, order.orderId, paid, stripePaymentConfirmationId);

    if (orderPaid) {
        yield put(new OrderStatusAction(OrderAction.Payment, paid ? Variant.Success : Variant.Warning, true))
        yield put(new SaveStatusAction(SaveStatus.Success));
    }
    else{
        yield put(new OrderStatusAction(OrderAction.Payment, Variant.Danger, true))
        yield put(new SaveStatusAction(SaveStatus.Failed))
    }
}

export function* setOrderDispatch() {
    const order : IOrder = yield select(getOrder);
    const dispatch : boolean = yield select(getOrderDispatch);

    if (dispatch && !order.dispatched || !dispatch && order.dispatched) {
        const orderDispatch: boolean = yield call(api.setOrderDispatch, order.orderId, dispatch);

        if (orderDispatch) {
            yield put(new OrderStatusAction(OrderAction.Dispatch, dispatch ? Variant.Success : Variant.Warning, false))
            yield put(new SaveStatusAction(SaveStatus.Success));
        }
        else{
            yield put(new OrderStatusAction(OrderAction.Dispatch, Variant.Danger, true))
            yield put(new SaveStatusAction(SaveStatus.Failed))
        }
    }
    else{
        yield put(new OrderStatusAction(OrderAction.Dispatch, dispatch ? Variant.Success : Variant.Warning, false))
    }
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
        yield put(new SetDeliveryDateAction(order.deliveryDate, order.timeslot))
        yield put(new OrderPaidAction(order.paid, order.stripePaymentConfirmationId))
        yield put(new DispatchOrderAction(order.dispatched))
    }
}

export function* saveOrder() {
    const orderForm : IOrderForm = yield select(getOrderForm)
    const orderOverview: IOrderOverview = yield select(getOrderOverview);
    const customer : IStakeholder = yield select(getSelectedCustomer);
    const service : IBaseModel = yield select(getSelectedService);
    const deliveryDate : Date = yield select(getDeliveryDate);
    const deliveryTimeslot : string = yield select(getDeliveryTimeslot);
    const paid : boolean = yield select(getOrderPaid);
    const dispatched : boolean = yield select(getOrderDispatch);
    const stripePaymentConfirmationId : string | undefined = yield select(getStripePaymentConfirmationId);

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
        driverEarning: orderOverview.netProfit - orderOverview.driverFee,
        deliveryDate: deliveryDate,
        timeslot: deliveryTimeslot,
        dispatched: dispatched,
        paid: paid,
        stripePaymentConfirmationId: stripePaymentConfirmationId
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

        // If creating order for first time set the order id in the order form to prevent repeating of order
        orderForm.orderId = orderResult.order.orderId
        yield put(new UpdateOrderAction(orderForm))

        // Create an action to dispatch on success with the returned entity from API
        const orderResultAction = new SaveOrderSuccessAction(orderResult.order, orderResult.trip);

        // Dispatch the new action with Redux
        yield put(orderResultAction);

        // Order status
        const orderStatus : SaveStatus = yield select(saveOrderStatus)
        const deliveryDateSet = deliveryDate && deliveryTimeslot;
        const orderSaved = orderStatus === SaveStatus.Success ? Variant.Success : Variant.Warning;

        yield put(new OrderStatusAction(OrderAction.DeliveryDate, deliveryDateSet ? Variant.Success : Variant.Warning, !deliveryDateSet))
        yield put(new OrderStatusAction(OrderAction.Save, orderSaved, true))

        if (!paid) {
            yield put(new OrderStatusAction(OrderAction.Payment, Variant.Warning, true))
        }

    
    } catch (e) {

        // Dispatch a failure action to Redux

        // Order status
        yield put(new OrderStatusAction(OrderAction.Save, Variant.Danger, true))
        yield put(new SaveOrderFailureAction(e.message));
        yield put(new ReportErrorAction(e.message));
        return;
    }

}