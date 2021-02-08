import { SelectElement } from "@appology/react-components";
import IBaseModel from "@appology/react-components/src/SelectionRefinement/IBaseModel";
import * as React from "react";
import { api } from "src/Api/Api";
import { IOrder } from "src/models/IOrder";
import { IStakeholder } from "src/models/IStakeholder";
import { SelectedServiceAction } from "src/state/contexts/landing/Actions";
import PickupLocationConnected from "../pickup/PickupLocationConnected";


export interface IPropsFromDispatch {
    selectedServiceChange: (service: IBaseModel | undefined) => SelectedServiceAction
}

export interface IPropsFromState {
    selectedCustomer?: IStakeholder
    selectedService?: IBaseModel,
    order?: IOrder
}

export interface IOwnState {
    services?: IBaseModel[], 
    loadingServices: boolean
}

export interface IOwnProps {
    newOrder: boolean
}

type AllProps = IPropsFromState & IPropsFromDispatch & IOwnProps;

export default class Service extends React.Component<AllProps, IOwnState> {

    constructor(props: any) {
        super(props);
        this.state = {
            services: [],
            loadingServices: false,
        }
    }

    public componentDidMount() {
        this.getServices();
    }

    public render(): JSX.Element {
        return (
            <>
                {
                    this.state.services !== undefined && (this.props.newOrder || this.props.order !== undefined)  ?
                        <div className="toggleswitch-margin-top">
                            <SelectElement 
                                label="Services"
                                selectorName="Select Service"
                                icon="&#xf108;"
                                loading={this.state.loadingServices}
                                id="service"
                                selected={this.props.selectedService?.id ?? ""}
                                selectorOptions={this.state.services}
                                onSelectChange={this.handleServiceChange}
                            />
                            <PickupLocationConnected />
                        </div>
                    : null
                }
            </>

        );
    }

    private handleServiceChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const service : IBaseModel = {
            id: e.target.value,
            name: e.target.name
        }

        this.props.selectedServiceChange(service);
    }

    private getServices() {
        this.setState({ loadingServices: true })

        api.services()
            .then((s) => this.servicesSuccess(s.services))
    }

    private servicesSuccess = (services: IBaseModel[]) => {
        this.setState({ 
            services: services,
            loadingServices: false
        })
    }
}