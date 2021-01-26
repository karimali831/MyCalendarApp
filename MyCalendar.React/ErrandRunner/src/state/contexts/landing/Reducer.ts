
import ILandingsState, { LandingState } from './ILandingState';
import { Reducer } from 'redux';
import { LandingActions, LandingActionTypes, /*LandingActionTypes*/ } from './Actions';
import { IStakeholderSearch } from 'src/models/IStakeholder';
import { stakeholderSearchTxt } from 'src/components/utils/Utils';
import { defaultConfig, defaultNavigator } from './Selectors';

const LandingReducer: Reducer<ILandingsState, LandingActions> =
    (state = LandingState.intialState, action) => {
        switch (action.type) {
            case LandingActionTypes.LoadStakeholders:
                return {
                    ...state,
                    ...{
                        filter: action.filter,
                        stakeholderId: action.stakeholderId,
                        loading: true
                    }
                }
              
            case LandingActionTypes.UpdateConfig:
                return {
                    ...state, ...{ 
                        config: action.config !== undefined ? action.config : defaultConfig 
                    }
                }  

            case LandingActionTypes.SelectedDriver:
                return {
                    ...state, ...{ 
                        selectedDriver: action.driver,
                        filter: "",
                        stakeholders: [],
                        // activeStep: action.driver !== undefined ? 4 : 3,
                        navigator: 
                            state.navigator.map((l, idx) => {
                                if (action.driver !== undefined) {
                                    if (idx === 3) {
                                        return { ...l,  ...{ label: `${action.driver?.firstName} Assigned` } }
                                    }
                                    else if (idx === 4) {
                                        return { ...l,  ...{ disabledMsg: "" } }
                                    } else {
                                        return { ...l }
                                    }
                                } 
                                else {
                                    return { ...l }
                                }
                            })
                    }
                }      

            case LandingActionTypes.SelectedCustomer:
                return {
                    ...state, ...{ 
                        selectedCustomer: action.customer,
                        filter: "",
                        stakeholders: [],
                        activeStep: action.customer !== undefined ? 1 : 0,
                        navigator: 
                            state.navigator.map((l, idx) => {
                                if (action.customer !== undefined) {
                                    if (idx === 0) {
                                        return { ...l,  ...{ label: `Deliver to ${action.customer.firstName}` } }
                                    }
                                    else if (idx === 1) {
                                        return { ...l,  ...{ disabledMsg: "" } }
                                    } else {
                                        return { ...l }
                                    }
                                } 
                                else {
                                    return { ...l }
                                }
                            })
                    }
                }    

            case LandingActionTypes.SelectedService:
                return {
                    ...state, ...{ 
                        selectedService: action.service,
                        activeStep: state.pickupPlace !== undefined && action.service?.id !== "" ? 2 : 1,
                        navigator: 
                            state.navigator.map((l, idx) => {
                                if (action.service?.id === "" && idx === 2) {
                                    return { ...l,  ...{ 
                                        disabledMsg: defaultNavigator[idx].disabledMsg } 
                                    }
                                } else {
                                    return { ...l }
                                }
                            }),
                    }
                }      

            case LandingActionTypes.ToggleDriverStep4:
                return {
                    ...state, ...{ 
                        navigator: 
                            state.navigator.map((l, idx) => {
                                if (idx === 3) {
                                    if (action.enable) {
                                        return { ...l,  ...{  disabledMsg: "" } }
                                    } else {
                                        return { ...l,  ...{  disabledMsg: defaultNavigator[idx].disabledMsg } }
                                    }
                                } else {
                                    return { ...l }
                                }
                            }),
                    }
                } 

            case LandingActionTypes.SelectedOrderEnableSteps:
                return {
                    ...state, ...{ 
                        activeStep: 2,
                        navigator: 
                            state.navigator.map(l => {
                                return { ...l,  
                                    ...{  disabledMsg: "" } 
                                }
                            }),
                    }
                } 

            case LandingActionTypes.UpdateTrip:
                return {
                    ...state, ...{ 
                        tripOverview: action.tripOverview,
                        activeStep: action.tripOverview !== undefined ? 2 : 1,
                        navigator: 
                            state.navigator.map((l, idx) => {
                                if (action.tripOverview !== undefined) {
                                    if (idx === 1) {
                                        return { ...l,  ...{ label: `Pickup from ${action.tripOverview.pickupPlace.split(' ')[0]}` } }
                                    } else if (idx === 2 && state.pickupPlace !== undefined && state.selectedService !== undefined) {
                                        return { ...l,  ...{ disabledMsg: "" } }
                                    } else {
                                        return { ...l }
                                    }
                                }
                                else {
                                    return { ...l }
                                }
                            }),
                    }
                }   
                
            case LandingActionTypes.DistanceMatrix:
                return {
                    ...state, ...{ 
                        pickupPlace: action.store,
                        pickupLocation: action.storeLocation,
                        stakeholderLocation: action.stakeholderLocation
                    }
                }  

            case LandingActionTypes.DistanceMatrixSuccess:
                return {
                    ...state, ...{  
                        activeStep: state.selectedService !== undefined ? 2 : 1 
                    }
                }     
                

            case LandingActionTypes.SetActiveStep:
                return {
                    ...state, ...{ 
                        activeStep: action.step,
                    }
                }     

            case LandingActionTypes.ResetOrder:
                return {
                    ...state, ...{ 
                        pickupPlace: undefined,
                        stakeholderLocation: undefined,
                        tripOverview: undefined,
                        selectedService: undefined,
                        selectedDriver: undefined,
                        activeStep: 1,
                        navigator: 
                            state.navigator.map(l  => {
                                if (l.stepNo !== 0) {
                                    return { ...l,  ...{ 
                                        label: defaultNavigator[l.stepNo].label,
                                        disabledMsg: l.stepNo !== 1 ? defaultNavigator[l.stepNo].disabledMsg : "" } 
                                    }
                                } else {
                                    return { ...l }
                                }
                        }),
                        orderForm: {
                            items: [{
                                name: "",
                                qty: 1,
                                cost: 0,
                                total: 0,
                                notes: ""
                            }],
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

            case LandingActionTypes.LoadStakeholdersSuccess:
                return {
                    ...state,
                    ...{
                        loading: false,
                        stakeholders: action.stakeholders.map(person =>({ 
                            stakeholder: person,
                            id: person.id, 
                            name: stakeholderSearchTxt(person)
                        } as IStakeholderSearch))
                    }
                }

            default:
                return state;
        }
    }

export default LandingReducer;