import { SelectElement, Variant, ToggleSwitch, Load } from '@appology/react-components';
import IBaseModel from '@appology/react-components/src/SelectionRefinement/IBaseModel';
import * as React from 'react';
import { IStakeholder, IStakeholderSearch } from 'src/models/IStakeholder';
import { ITrip, ITripOverview } from 'src/models/ITrip';
import OrderConnected  from '../order/OrderConnected';
import { SidebarMenu } from 'src/components/menu/SidebarMenu';
import { IDefaultConfig } from 'src/models/IDefaultConfig';
import {  ResetOrderAction, SelectedDriverAction, SelectedServiceAction, UpdateConfigAction } from 'src/state/contexts/landing/Actions';
import NavigatorConnected from 'src/components/menu/NavigatorConnected';
import PickupLocationConnected from '../storelocator/PickupLocationConnected';
import RegistrationConnected from '../registration/RegistrationConnected';
import { api, IOrderResponse } from 'src/Api/Api';
import { IOrder } from 'src/models/IOrder';
import { SelectedOrderAction } from 'src/state/contexts/order/Actions';
import OverviewConnected from '../step5/OverviewConnected';
import { showAlert } from 'src/components/utils/Utils';

export interface IPropsFromDispatch {
    updateConfig: (config: IDefaultConfig | undefined) => UpdateConfigAction,
    resetOrder: () => ResetOrderAction,
    selectedServiceChange: (service: IBaseModel | undefined) => SelectedServiceAction,
    selectedDriverChange: (stakeholder: IStakeholder | undefined) => SelectedDriverAction,
    selectedOrderChange: (order: IOrder | undefined, trip: ITrip | undefined) => SelectedOrderAction
}

export interface IPropsFromState {
    tripOverview?: ITripOverview,
    selectedCustomer?: IStakeholder,
    selectedService?: IBaseModel,
    config: IDefaultConfig,
    pinSidebar: boolean,
    stakeholders: IStakeholderSearch[],
    filter: string,
    step: number,
    order?: IOrder
}

export interface IOwnState {
    newOrder: boolean,
    services?: IBaseModel[], 
    loadingServices: boolean,
    loadingOrder: boolean,
    orders?: IBaseModel[]
}

type AllProps = IPropsFromState & IPropsFromDispatch;

export default class Landing extends React.Component<AllProps, IOwnState> {

    constructor(props: AllProps) {
        super(props);
        this.state = {
            newOrder: true,
            services: undefined,
            orders: undefined,
            loadingServices: false,
            loadingOrder: false,
        };
    }

    public componentDidUpdate = (prevProps: AllProps, prevState: IOwnState) => {
        if (JSON.stringify(this.props.config) !== JSON.stringify(this.props.config)) {
            this.props.updateConfig(this.props.config)
        }

        if (this.props.selectedCustomer !== undefined && this.props.step === 1) {
            if (this.state.services === undefined) {
                this.getServices();
            }

            if (this.state.orders === undefined) {
                this.getOrders();
            }

            // reset order if order selected then making new order
            if (prevState.newOrder !== this.state.newOrder) {
                this.props.resetOrder();
                if (this.props.order !== undefined) {
                    this.props.selectedOrderChange(undefined, undefined);
                }
            }

            // reset order if changing customer
            if (prevProps.selectedCustomer !== undefined && this.props.selectedCustomer?.id !== prevProps.selectedCustomer?.id) {
                this.props.resetOrder();
                this.getOrders();
            }
        }
    }

    public render() {

        if (this.state.loadingOrder) {
            return <Load withBackground={true} />
        }

        return (
            <div>
                <SidebarMenu 
                    initialState={() => this.props.updateConfig(undefined)}
                    pinSidebar={this.props.pinSidebar && this.props.step === 2} 
                    config={this.props.config}
                    updateConfig={this.props.updateConfig}
                />  
                <div className={this.props.pinSidebar && this.props.step === 2 ? "sidebar-margin" : ""}>
                    <div className="wrap-login100 p-l-55 p-r-55 p-t-65 p-b-54">
                        <NavigatorConnected />
                        {
                            this.props.step === 0 || this.props.step === 3 ? <>
                                <RegistrationConnected />
                            </>
                            : this.props.step === 1 && this.props.selectedCustomer !== undefined ?
                                <>
                                    {
                                        this.state.orders !== undefined ?
                                            <>
                                                {
                                                    !this.state.newOrder && this.state.orders.length > 0 ?
                                                        <SelectElement 
                                                            label="Existing orders"
                                                            selectorName="Select Order"
                                                            icon="&#xf1cb;"
                                                            id="order"
                                                            selected={this.props.order?.orderId}
                                                            selectorOptions={this.state.orders}
                                                            onSelectChange={this.handleOrderChange}
                                                        />
                                                    : null
                                                }
                                                <ToggleSwitch id="newOrder" name='New order' onChange={c => this.newOrder(c)} checked={this.state.newOrder} />
                                                {  
                                                    this.state.services !== undefined && (this.state.newOrder || this.props.order !== undefined)  ?
                                                        <div className="toggleswitch-margin-top">
                                                            <SelectElement 
                                                                label="Services"
                                                                selectorName="Select Service"
                                                                icon="&#xf108;"
                                                                loading={this.state.loadingServices}
                                                                id="service"
                                                                selected={this.props.selectedService?.id.toString()}
                                                                selectorOptions={this.state.services}
                                                                onSelectChange={this.handleServiceChange}
                                                            />
                                                            <PickupLocationConnected />
                                                        </div>
                                                    : null
                                                }
                                            </>
                                        : null
                                    }
                                </>
                            : this.props.step === 2 ?
                                <OrderConnected />  
                            : this.props.step === 4 ?
                                <OverviewConnected />
                            : null
                        }
                    </div>
                </div>
            </div>
        );
    }    

    private newOrder = (checked: boolean) => {
        this.setState({ newOrder: checked })
    }

    private getServices() {
        this.setState({ 
            loadingServices: true,
            services: []
        })

        api.services()
            .then((s) => this.servicesSuccess(s.services))
    }

    private servicesSuccess = (services: IBaseModel[]) => {
        this.setState({ 
            services: services,
            loadingServices: false
        })
    }

    private getOrders() {
        this.setState({ orders: [] })

        if (this.props.selectedCustomer !== undefined) {
            api.orders(this.props.selectedCustomer?.id)
                .then((s) => this.ordersSuccess(s.orders));
        }

    }

    private ordersSuccess = (orders: IBaseModel[]) => {
        this.setState({ 
            orders: orders,
            newOrder: orders.length === 0
        })
    }

    private handleServiceChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const service : IBaseModel = {
            id: e.target.value,
            name: e.target.name
        }

        this.props.selectedServiceChange(service);
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
        this.setState({ loadingOrder: false })

        if (order.status) {
            this.props.selectedDriverChange(order.driver);
            this.props.selectedOrderChange(order.order, order.trip);
        }
        else{
        
            showAlert("Unable to load order", Variant.Danger);
        }
    }
}