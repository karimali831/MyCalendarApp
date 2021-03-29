import IBaseModel from "@appology/react-components/src/SelectionRefinement/IBaseModel";
import { IGoogleDistanceMatrixRows, IGoogleGeoLocation } from "src/Api/GoogleApi";
import { IStakeholder } from "src/models/IStakeholder";
import { IDefaultConfig } from "src/models/IDefaultConfig";
import { IGoogleAutoCompleteSearch } from "src/models/IGoogleAutoComplete";
import { ITripOverview } from "src/models/ITrip";
import { Stakeholders } from "src/Enums/Stakeholders";
import { Variant } from "@appology/react-components";
import { IPlace } from "src/models/IPlace";

// action types
export class LandingActionTypes {
    public static readonly LoadStakeholders = "@@landing/loadstakeholders";
    public static readonly LoadStakeholdersSuccess = "@@landing/loadstakeholderssuccess";
    public static readonly LoadStakeholdersFailure = "@@landing/loadstakeholdersfailure";
    public static readonly UpdateConfig = "@@landing/updateconfig";
    public static readonly UpdateTrip = "@@landing/updatetrip"
    public static readonly SetActiveStep = "@@landing/setactivestep"
    public static readonly SelectedCustomer = "@@landing/selectedcustomer";
    public static readonly SelectedDriver = "@@landing/selectedriver";
    public static readonly SelectedService = "@@landing/selectedservice"
    public static readonly DistanceMatrix = "@@landing/distancematrix";
    public static readonly DistanceMatrixSuccess = "@@landing/distancematrixsuccess";
    public static readonly DistanceMatrixFailure = "@@landing/distancematrixfailure";
    public static readonly ResetOrder = "@@landing/resetorder";
    public static readonly ToggleDriverStep4 = "@@landing/toggledriverstep4";
    public static readonly SelectedOrderEnableSteps = "@@landing/seletedorderenablesteps";
    public static readonly ShowAlert = "@@landing/showalert";
    public static readonly Place = "@@landing/place"
}

export class LoadStakeholdersAction {
    public static readonly creator = (filter: string, stakeholderId: Stakeholders) => new LoadStakeholdersAction(filter, stakeholderId);

    public readonly type = LandingActionTypes.LoadStakeholders;

    constructor(
        public filter: string,
        public stakeholderId: Stakeholders
    ) { }
}

export class ToggleAlertAction {
    public static readonly creator = (text: string, variant?: Variant, timeout?: number) => new ToggleAlertAction(text, variant, timeout);

    public readonly type = LandingActionTypes.ShowAlert;

    constructor(
        public text: string,
        public variant?: Variant,
        public timeout?: number
    ) { }
}

export class UpdateConfigAction {
    public static readonly creator = (config: IDefaultConfig | undefined) => new UpdateConfigAction(config);

    public readonly type = LandingActionTypes.UpdateConfig;

    constructor(
        public config: IDefaultConfig | undefined
    ) { }
}

export class SetActiveStepAction {
    public static readonly creator = (step: number) => new SetActiveStepAction(step);

    public readonly type = LandingActionTypes.SetActiveStep;

    constructor(
        public step: number
    ) { }
}

export class ResetOrderAction {
    public static readonly creator = () => new ResetOrderAction();

    public readonly type = LandingActionTypes.ResetOrder;
}

export class SelectedServiceAction {
    public static readonly creator = (service: IBaseModel | undefined) => new SelectedServiceAction(service);

    public readonly type = LandingActionTypes.SelectedService;

    constructor(
        public service: IBaseModel | undefined
    ) { }
}

export class SelectedCustomerAction {
    public static readonly creator = (customer: IStakeholder | undefined) => new SelectedCustomerAction(customer);

    public readonly type = LandingActionTypes.SelectedCustomer;

    constructor(
        public customer: IStakeholder | undefined
    ) { }
}

export class SelectedDriverAction {
    public static readonly creator = (driver: IStakeholder | undefined) => new SelectedDriverAction(driver);

    public readonly type = LandingActionTypes.SelectedDriver;

    constructor(
        public driver: IStakeholder | undefined
    ) { }
}

export class LoadStakeholdersSuccessAction {
    public static readonly creator = (stakeholders: IStakeholder[]) => new LoadStakeholdersSuccessAction(stakeholders);

    public readonly type = LandingActionTypes.LoadStakeholdersSuccess;

    constructor(
        public stakeholders: IStakeholder[]
    ) { }
}

export class UpdateTripAction {
    public static readonly creator = (tripOverview: ITripOverview | undefined) => new UpdateTripAction(tripOverview);

    public readonly type = LandingActionTypes.UpdateTrip;

    constructor(
        public tripOverview: ITripOverview | undefined
    ) { }
}

export class LoadStakeholdersFailureAction {
    public static readonly creator = (errorMsg: string) => new LoadStakeholdersFailureAction(errorMsg);

    public readonly type = LandingActionTypes.LoadStakeholdersFailure;

    constructor(
        public errorMsg: string
    ) { }
}

export class DistanceMatrixAction {
    public static readonly creator = (store: IGoogleAutoCompleteSearch, storeLocation: IGoogleGeoLocation, stakeholderLocation: IGoogleGeoLocation) => new DistanceMatrixAction(store, storeLocation, stakeholderLocation);

    public readonly type = LandingActionTypes.DistanceMatrix;

    constructor(
        public store: IGoogleAutoCompleteSearch,
        public storeLocation: IGoogleGeoLocation,
        public stakeholderLocation: IGoogleGeoLocation
    ) { }
}

export class PlaceAction {
    public static readonly creator = (place: IPlace | undefined) => new PlaceAction(place);

    public readonly type = LandingActionTypes.Place;

    constructor(
        public place: IPlace | undefined
    ) { }
}

export class DistanceMatrixSuccessAction {
    public static readonly creator = (matrix: IGoogleDistanceMatrixRows[]) => new DistanceMatrixSuccessAction(matrix);

    public readonly type = LandingActionTypes.DistanceMatrixSuccess;

    constructor(
        public matrix: IGoogleDistanceMatrixRows[]
    ) { }
}

export class DistanceMatrixFailureAction {
    public static readonly creator = (errorMsg: string) => new DistanceMatrixFailureAction(errorMsg);

    public readonly type = LandingActionTypes.DistanceMatrixFailure

    constructor(
        public errorMsg: string
    ) { }
}

export class ToggleDriverStep4Action {
    public static readonly creator = (enable: boolean) => new ToggleDriverStep4Action(enable);

    public readonly type = LandingActionTypes.ToggleDriverStep4;

    constructor(
        public enable: boolean
    ) { }
}

export class SelectedOrderEnableStepsAction {
    public static readonly creator = () => new  SelectedOrderEnableStepsAction();

    public readonly type = LandingActionTypes.SelectedOrderEnableSteps;
}

// Create a discriminated union of all action types used to correctly type the
// actions in the reducer switch statement
export type LandingActions =
    LoadStakeholdersAction |
    LoadStakeholdersSuccessAction |
    LoadStakeholdersFailureAction |
    UpdateConfigAction |
    UpdateTripAction |
    SetActiveStepAction |
    SelectedCustomerAction |
    SelectedDriverAction |
    SelectedServiceAction |
    DistanceMatrixAction |
    DistanceMatrixSuccessAction |
    DistanceMatrixFailureAction |
    ResetOrderAction |
    ToggleDriverStep4Action |
    SelectedOrderEnableStepsAction |
    ToggleAlertAction |
    PlaceAction