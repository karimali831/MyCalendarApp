import { Variant, ToggleSwitch, AlertModal } from '@appology/react-components';
import * as React from 'react';
import { IStakeholder } from 'src/models/IStakeholder';
import { SidebarMenu } from 'src/components/menu/SidebarMenu';
import { IDefaultConfig } from 'src/models/IDefaultConfig';
import {  ResetOrderAction, ToggleAlertAction, UpdateConfigAction } from 'src/state/contexts/landing/Actions';
import NavigatorConnected from 'src/components/menu/NavigatorConnected';
import RegistrationConnected from '../step1-customer/RegistrationConnected';
import OverviewConnected from '../step5-stripe/OverviewConnected';
import OrderConnected from '../step3-order/order/OrderConnected';
import ExistingOrdersConnected from '../step2-service/existingorders/ExistingOrdersConnected';
import ServiceConnected from '../step2-service/service/ServiceConnected';
import { IOrder } from 'src/models/IOrder';

export interface IPropsFromDispatch {
    updateConfig: (config: IDefaultConfig | undefined) => UpdateConfigAction,
    resetOrder: () => ResetOrderAction,
    handleAlert: (text: string, variant?: Variant, timeout?: number) => ToggleAlertAction
}

export interface IPropsFromState {
    selectedCustomer?: IStakeholder,
    selectedOrder?: IOrder,
    config: IDefaultConfig,
    pinSidebar: boolean,
    step: number,
    alertTxt: string,
    alertVariant: Variant,
    alertTimeout?: number
}

export interface IOwnState {
    newOrder: boolean
}

type AllProps = IPropsFromState & IPropsFromDispatch;

export default class Landing extends React.Component<AllProps, IOwnState> {

    constructor(props: AllProps) {
        super(props);
        this.state = {
            newOrder: true
        };
    }

    public componentDidUpdate = (prevProps: AllProps, prevState: IOwnState) => {
        if (JSON.stringify(this.props.config) !== JSON.stringify(this.props.config)) {
            this.props.updateConfig(this.props.config)
        }

        if (this.props.selectedOrder !== undefined && this.state.newOrder === true) {
            this.setState({ newOrder: false })
        }

        if (this.props.selectedCustomer !== undefined && this.props.step === 1) {
            // reset order if changing customer
            if (prevProps.selectedCustomer !== undefined && this.props.selectedCustomer?.id !== prevProps.selectedCustomer?.id) {
                this.props.resetOrder();
            }
        }
    }

    public render() {
        return (
            <div>
                {
                    this.props.alertTxt !== "" ?
                        <AlertModal 
                            show={true} 
                            text={this.props.alertTxt} 
                            timeout={this.props.alertTimeout}
                            handleClose={() => this.props.handleAlert("")} 
                            variant={this.props.alertVariant} />
                    : null
                }   
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
                            this.props.step === 0 || this.props.step === 3 ?
                                <RegistrationConnected />
                            : this.props.step === 1 ?
                                <>
                                    <ExistingOrdersConnected newOrder={this.state.newOrder}  />
                                    <ToggleSwitch id="newOrder" name='New order' onChange={c => this.newOrder(c)} checked={this.state.newOrder} />
                                    <ServiceConnected newOrder={this.state.newOrder} />
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
        this.props.resetOrder();
        this.setState({ newOrder: checked })
    }
}