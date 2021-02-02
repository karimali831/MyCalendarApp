
import IOrdersState, { OrderState } from './IOrderState';
import { Reducer } from 'redux';
import { OrderActions, OrderActionTypes } from './Actions';

const OrderReducer: Reducer<IOrdersState, OrderActions> =
    (state = OrderState.intialState, action) => {
        switch (action.type) {
            case OrderActionTypes.ToggleConfig:
                return {
                    ...state,
                    ...{
                        pinSidebar: !state.pinSidebar
                    }
                }

            case OrderActionTypes.UpdateOrder:
                return { ...state, 
                    ...{ 
                        orderForm: action.order,
                    } 
                };

            case OrderActionTypes.SaveOrder:
                return { ...state,  ...{  saveOrderLoading: true } };   

            case OrderActionTypes.SaveOrderSuccess:
                return { ...state, 
                    ...{ 
                        order: action.order,
                        trip: action.trip,
                        saveOrderLoading: false,
                        saveOrderStatus: true
                    } 
                };    

            case OrderActionTypes.SaveOrderFailure:
                return { ...state,  
                    ...{ 
                        saveOrderLoading: false,
                        saveOrderStatus: false
                    } 
                };      

            case OrderActionTypes.OrderOverview:
                return { ...state, 
                    ...{ 
                        orderOverview: action.order,
                    } 
                };    

            case OrderActionTypes.SelectedOrder:
                return { ...state, 
                    ...{ 
                        order: action.order,
                        trip: action.trip
                    } 
                };   

            default:
                return state;
        }
    }

export default OrderReducer;