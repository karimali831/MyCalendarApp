import IBaseModel from "@appology/react-components/src/SelectionRefinement/IBaseModel";
import { IGoogleGeoLocation } from "src/Api/GoogleApi";
import { IStakeholder, IStakeholderSearch } from "src/models/IStakeholder";
import { IDefaultConfig } from "src/models/IDefaultConfig";
import { IGoogleAutoCompleteSearch } from "src/models/IGoogleAutoComplete";
import { INavigator } from "src/models/INavigator";
import { ITripOverview } from "src/models/ITrip";
import { defaultConfig, defaultNavigator, defaultTimeout } from "./Selectors";
import { Stakeholders } from "src/Enums/Stakeholders";
import { Variant } from "@appology/react-components";

export default interface ILandingState {
    config: IDefaultConfig,
    filter: string,
    stakeholderId: Stakeholders,
    stakeholders: IStakeholderSearch[],
    activeStep: number,
    navigator: INavigator[],
    loading: boolean,
    tripOverview?: ITripOverview,
    stakeholderLocation?: IGoogleGeoLocation,
    pickupLocation?: IGoogleGeoLocation,
    pickupPlace?: IGoogleAutoCompleteSearch,
    selectedCustomer?: IStakeholder,
    selectedDriver?: IStakeholder,
    selectedService?: IBaseModel,
    alertTxt: string,
    alertVariant?: Variant,
    alertTimeout?: number
}

export class LandingState {
    public static readonly intialState = {
        config: defaultConfig,
        filter: "",
        stakeholderId: Stakeholders.customer,
        stakeholders: [],
        activeStep: 0,
        navigator: defaultNavigator,
        tripOverview: undefined,
        loading: false,
        stakeholderLocation: undefined,
        pickupPlace: undefined,
        selectedCustomer: undefined,
        selectedService: undefined,
        alertTxt: "",
        alertVariant: Variant.Success,
        alertTimeout: defaultTimeout
    }
}