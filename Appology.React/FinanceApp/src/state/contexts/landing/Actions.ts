import { ISpendingSummary } from '../../../models/ISpending';
import { IDateFilter } from "../../../models/IDateFilter";
import { IIncomeSummary } from 'src/models/IIncome';
import { IReminderNotification } from 'src/models/IReminder';

// action types
export class LandingSummaryActionTypes {
    public static readonly LoadSpendingSummary = "@@landingsummary/loadspendingsummary";
    public static readonly LoadSpendingSummarySuccess = "@@landingsummary/loadspendingsummarysuccess";
    public static readonly LoadSpendingSummaryFailure = "@@landingsummary/loadspendingsummaryfailure";
    public static readonly LoadIncomeSummary = "@@landingsummary/loadincomesummary";
    public static readonly LoadIncomeSummarySuccess = "@@landingsummary/loadincomesummarysuccess";
    public static readonly LoadIncomeSummaryFailure = "@@landingsummary/loadincomesummaryfailure";
    public static readonly ShowSecondCategorySpendingSummary = "@@landingsummary/showsecondcategoryspendingsummary";
    public static readonly ShowSecondCategoryIncomeSummary = "@@landingsummary/showsecondcategoryincomesummary";
    public static readonly SpendingSummaryDateFilterChange = "@@landingsummary/spendingsummarydatefilterchange"
    public static readonly IncomeSummaryDateFilterChange = "@@landingsummary/incomesummarydatefilterchange"
    public static readonly LoadNotifications = "@@landingsummary/loadnotifications";
    public static readonly LoadNotificationsSuccess = "@@landingsummary/loadnotificationssuccess";
    public static readonly LoadNotificationsFailure = "@@landingsummary/loadnotificationsfailure";
    public static readonly FilterChanged = "@@landingsummary/filterchanged";
    public static readonly SpendingSummaryMaxCatsChange = "@@common/spendingsummarymaxcatschange"
}

// actions defined as classes with a creator as a static function
export class LoadSpendingSummaryAction {
    public static readonly creator = (dateFilter: IDateFilter) => new LoadSpendingSummaryAction(dateFilter);

    public readonly type = LandingSummaryActionTypes.LoadSpendingSummary;

    constructor(
        public dateFilter: IDateFilter
    ) { }
}

export class LoadSpendingSummarySuccessAction {
    public static readonly creator = (spendingSummary: ISpendingSummary[], spendingSummaryOverview: ISpendingSummary[], totalSpent: number) => new LoadSpendingSummarySuccessAction(spendingSummary, spendingSummaryOverview, totalSpent);

    public readonly type = LandingSummaryActionTypes.LoadSpendingSummarySuccess;

    constructor(
        public spendingSummary: ISpendingSummary[], 
        public spendingSummaryOverview: ISpendingSummary[],
        public totalSpent: number
    ) { }
}

export class LoadSpendingSummaryFailureAction {
    public static readonly creator = (errorMsg: string) => new LoadSpendingSummaryFailureAction(errorMsg);

    public readonly type = LandingSummaryActionTypes.LoadSpendingSummaryFailure;

    constructor(
        public errorMsg: string
    ) { }
}

export class LoadIncomeSummaryAction {
    public static readonly creator = (dateFilter: IDateFilter) => new LoadIncomeSummaryAction(dateFilter);

    public readonly type = LandingSummaryActionTypes.LoadIncomeSummary;

    constructor(
        public dateFilter: IDateFilter
    ) { }
}

export class LoadIncomeSummarySuccessAction {
    public static readonly creator = (incomeSummary: IIncomeSummary[], totalIncome: number) => new LoadIncomeSummarySuccessAction(incomeSummary, totalIncome);

    public readonly type = LandingSummaryActionTypes.LoadIncomeSummarySuccess;

    constructor(
        public incomeSummary: IIncomeSummary[], 
        public totalIncome: number
    ) { }
}

export class LoadIncomeSummaryFailureAction {
    public static readonly creator = (errorMsg: string) => new LoadIncomeSummaryFailureAction(errorMsg);

    public readonly type = LandingSummaryActionTypes.LoadIncomeSummaryFailure;

    constructor(
        public errorMsg: string
    ) { }
}

export class ShowSecondCategorySpendingSummaryAction {
    public static readonly creator = (secondCat: string) => new ShowSecondCategorySpendingSummaryAction(secondCat);

    public readonly type = LandingSummaryActionTypes.ShowSecondCategorySpendingSummary;

    constructor(
        public secondCat: string
    ) { }
}

export class ShowSecondCategoryIncomeSummaryAction {
    public static readonly creator = (secondCat: string) => new ShowSecondCategoryIncomeSummaryAction(secondCat);

    public readonly type = LandingSummaryActionTypes.ShowSecondCategoryIncomeSummary;

    constructor(
        public secondCat: string
    ) { }
}

export class SpendingSummaryDateFilterChangeAction {
    public static readonly creator = (filter: IDateFilter) => new SpendingSummaryDateFilterChangeAction(filter);

    public readonly type = LandingSummaryActionTypes.SpendingSummaryDateFilterChange

    constructor(
        public filter: IDateFilter 
    ) { }
}

export class SpendingSummaryMaxCatsChangeAction {
    public static readonly creator = (maxCats: number) => new SpendingSummaryMaxCatsChangeAction(maxCats);

    public readonly type = LandingSummaryActionTypes.SpendingSummaryMaxCatsChange

    constructor(
        public maxCats: number
    ) { }
}


export class IncomeSummaryDateFilterChangeAction {
    public static readonly creator = (filter: IDateFilter) => new IncomeSummaryDateFilterChangeAction(filter);

    public readonly type = LandingSummaryActionTypes.IncomeSummaryDateFilterChange

    constructor(
        public filter: IDateFilter 
    ) { }
}

export class LoadNotificationsAction {
    public static readonly creator = () => new LoadNotificationsAction();

    public readonly type = LandingSummaryActionTypes.LoadNotifications
}

export class LoadNotificationsSuccessAction {
    public static readonly creator = (notifications: IReminderNotification) => new LoadNotificationsSuccessAction(notifications);

    public readonly type = LandingSummaryActionTypes.LoadNotificationsSuccess;

    constructor(
        public notifications: IReminderNotification
    ) { }
}

export class LoadNotificationsFailureAction {
    public static readonly creator = (errorMsg: string) => new LoadNotificationsFailureAction(errorMsg);

    public readonly type = LandingSummaryActionTypes.LoadNotificationsFailure;

    constructor(
        public errorMsg: string
    ) { }
}

export class FilterChangedAction {
    public static readonly creator = (filter: string) => new FilterChangedAction(filter);

    public readonly type = LandingSummaryActionTypes.FilterChanged;

    constructor(
        public filter: string,
    ) { }
}

// Create a discriminated union of all action types used to correctly type the
// actions in the reducer switch statement
export type LandingSummaryActions =
    LoadSpendingSummaryAction |
    LoadSpendingSummarySuccessAction |
    LoadSpendingSummaryFailureAction |
    LoadIncomeSummaryAction |
    LoadIncomeSummarySuccessAction |
    LoadIncomeSummaryFailureAction |
    ShowSecondCategorySpendingSummaryAction |
    ShowSecondCategoryIncomeSummaryAction |
    SpendingSummaryDateFilterChangeAction |
    IncomeSummaryDateFilterChangeAction |
    LoadNotificationsAction |
    LoadNotificationsSuccessAction |
    LoadNotificationsFailureAction  |
    FilterChangedAction |
    SpendingSummaryMaxCatsChangeAction