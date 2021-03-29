import { IGoogleGeoLocation } from "src/Api/GoogleApi";
import { IStakeholder } from "src/models/IStakeholder";
import { IDefaultConfig } from "src/models/IDefaultConfig";
import { IGoogleAutoCompleteSearch } from "src/models/IGoogleAutoComplete";
import { INavigator } from "src/models/INavigator";
import { ITripOverview } from "src/models/ITrip";
import IStoreState from "src/state/IStoreState";
import { Stakeholders } from "src/Enums/Stakeholders";
import IBaseModel from "@appology/react-components/src/SelectionRefinement/IBaseModel";

export const getStakeholderSearchFilter = (state: IStoreState): string => {
    return state.landing.filter
}

export const getStakeholderId = (state: IStoreState): Stakeholders => {
    return state.landing.stakeholderId
}

export const getStakeholderLocation = (state: IStoreState): IGoogleGeoLocation | undefined => {
    return state.landing.stakeholderLocation
}

export const getPickupLocation = (state: IStoreState): IGoogleAutoCompleteSearch | undefined => {
    return state.landing.pickupPlace
}

export const getSelectedCustomer = (state: IStoreState): IStakeholder | undefined => {
    return state.landing.selectedCustomer
}

export const getSelectedDriver = (state: IStoreState): IStakeholder | undefined => {
    return state.landing.selectedDriver
}

export const getPickupGeometry = (state: IStoreState): IGoogleGeoLocation | undefined => {
    return state.landing.pickupLocation
}

export const getSelectedService = (state: IStoreState): IBaseModel | undefined => {
    return state.landing.selectedService
}

export const getTripOverview = (state: IStoreState) : ITripOverview | undefined => {
    return state.landing.tripOverview;
}

export const getConfig = (state: IStoreState) : IDefaultConfig => {
    return state.landing.config
}

export const getAlertTimeout = (state: IStoreState) : number => {
    return state.landing.alertTimeout ?? defaultTimeout
}

export const defaultTimeout : number = 1200;

export const defaultNavigator : INavigator[] = [
    {
        stepNo: 0,
        label: 'Customer', 
        disabledMsg: ""
    }, 
    {
        stepNo: 1,
        label: 'Order', 
        disabledMsg: "Select or register a customer first"
    },
    {
        stepNo: 2,
        label: 'Basket', 
        disabledMsg: "Select an existing or new order first"
    }, 
    {
        stepNo: 3,
        label: 'Driver', 
        disabledMsg: "Minimum order value not met",
    },
    {
        stepNo: 4,
        label: 'Payment', 
        disabledMsg: "Assign a delivery driver for the trip"
    }
]

export const defaultConfig : IDefaultConfig = {     
    orderFeeFormula: [
        { 
            orderValueMin: 0, 
            orderValueMax: 9.99, 
            fee: 0.4 
        },
        { 
            orderValueMin: 10, 
            orderValueMax: 19.99, 
            fee: 0.3 
        },
        { 
            orderValueMin: 20, 
            orderValueMax: 500, 
            fee: 0.2 
        }
    ],
    serviceFee: 0.15,
    deliveryFeePerMile: 1.5, 
    driverFee: 0.3,
    minimumOrderValue: 15
}