
import IOrdersState, { OrderState } from './IOrderState';
import { Reducer } from 'redux';
import { OrderActions, OrderActionTypes } from './Actions';
import { SaveStatus } from 'src/Enums/SaveStatus';

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
                return { ...state,  ...{  
                    saveOrderStatus: action.saved ? SaveStatus.Unprocessing : SaveStatus.Processing,
                } };   

            case OrderActionTypes.SaveOrderSuccess:
                return { ...state, 
                    ...{ 
                        order: action.order,
                        trip: action.trip,
                        saveOrderStatus: SaveStatus.Success
                    } 
                };    

            case OrderActionTypes.SaveOrderFailure:
                return { ...state,  
                    ...{ 
                        saveOrderStatus: SaveStatus.Failed
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

                case OrderActionTypes.ResetOrder:
                    return {
                        ...state, ...{ 
                            order: undefined,
                            trip: undefined,
                            orderForm: {
                                items: [{
                                    name: "",
                                    qty: 1,
                                    cost: 0,
                                    total: 0,
                                    notes: ""
                                }],
                                orderId: "",
                                orderValue: 0,
                                orderFee: 0,
                                totalItems: 1,
                    
                            },
                            orderOverview: {
                                tripMiles: 0,
                                deliveryFee: 0,
                                serviceFee: 0,
                                invoiceAmt: 0,
                                netProfit: 0,
                                driverFee: 0
                            },
                        }
                    }      

            default:
                return state;
        }
    }

export default OrderReducer;