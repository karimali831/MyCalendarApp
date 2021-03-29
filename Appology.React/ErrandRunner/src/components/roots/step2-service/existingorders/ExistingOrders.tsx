import { Load, SelectElement, Variant } from '@appology/react-components';
import IBaseModel from '@appology/react-components/src/SelectionRefinement/IBaseModel';
import * as React from 'react';
import { api, IOrderResponse } from 'src/Api/Api';
import { IOrder } from 'src/models/IOrder';
import { IPlace } from 'src/models/IPlace';
import { IStakeholder } from 'src/models/IStakeholder';
import { ITrip } from 'src/models/ITrip';
import { PlaceAction, ResetOrderAction, SelectedDriverAction, ToggleAlertAction } from 'src/state/contexts/landing/Actions';
import { SelectedOrderAction } from 'src/state/contexts/order/Actions';

export interface IPropsFromDispatch {
    handleAlert: (text: string, variant?: Variant, timeout?: number) => ToggleAlertAction,
    selectedOrderChange: (order: IOrder | undefined, trip: ITrip | undefined) => SelectedOrderAction,
    selectedDriverChange: (stakeholder: IStakeholder | undefined) => SelectedDriverAction,
    onPlaceChange: (place: IPlace | undefined) => PlaceAction,
    resetOrder: () => ResetOrderAction
}

export interface IPropsFromState {
    selectedCustomer?: IStakeholder,
    order?: IOrder
}

export interface IOwnState {
    orders: IBaseModel[],
    loadingOrders?: boolean,
    loadingOrder: boolean
}

export interface IOwnProps {
    newOrder: boolean
}

export interface IOwnState {
}

type AllProps = IPropsFromState & IPropsFromDispatch & IOwnProps

export default class ExistingOrders extends React.Component<AllProps, IOwnState> {

    constructor(props: AllProps) {
        super(props);

        this.state = {
            orders: [],
            loadingOrder: false,
            loadingOrders: undefined
        };
    }

    public componentDidMount() {
        this.getOrders();
    }

    public componentDidUpdate = (prevProps: AllProps, prevState: IOwnState) => {
        // re-populate orders list if changing customer
        if (prevProps.selectedCustomer !== undefined && this.props.selectedCustomer?.id !== prevProps.selectedCustomer?.id) {
            this.getOrders();
        }
    }

    public render() {

        if (this.state.loadingOrder) {
            return <Load withBackground={true} />
        }

        return (
            <SelectElement 
                label="Existing orders"
                selectorName={this.state.loadingOrders === undefined ? "..." : this.state.orders.length === 0 ? "No previous orders" : "Select Order"}
                icon="&#xf1cb;"
                id="order"
                loading={this.state.loadingOrders}
                selected={this.props.newOrder ? "" : this.props.order?.orderId}
                selectorOptions={this.state.orders}
                onSelectChange={this.handleOrderChange}
            />
        );
    }

    private getOrders() {
        this.setState({ 
            orders: [],
            loadingOrders: true 
        })

        if (this.props.selectedCustomer !== undefined) {
            api.orders(this.props.selectedCustomer?.id)
                .then((s) => this.ordersSuccess(s.orders));
        }

    }

    private ordersSuccess = (orders: IBaseModel[]) => {
        this.setState({ 
            orders: orders,
            loadingOrders: false 
        })
    }


    private handleOrderChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        if (e.target.value !== "") { 
            this.setState({ loadingOrder: true })

            api.order(e.target.value)
                .then(c => this.orderSuccess(c));
        }
        else {
            this.props.selectedOrderChange(undefined, undefined);
            this.props.resetOrder();
        }
    }

    private orderSuccess = (order: IOrderResponse) => {

        // this is to see if we have any API data in ER.Places
        api.place(order.trip.pickupId).then(place => this.props.onPlaceChange(place ?? undefined))

        this.setState({ loadingOrder: false })

        if (order.status) {
            this.props.selectedDriverChange(order.driver);
            this.props.selectedOrderChange(order.order, order.trip);
        }
        else{
        
            this.props.handleAlert("Unable to load order", Variant.Danger);
        }
    }

}