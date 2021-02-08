import { SelectElement  } from '@appology/react-components';
import IBaseModel from '@appology/react-components/src/SelectionRefinement/IBaseModel';
import * as React from 'react';
import { api } from 'src/Api/Api';
import { IGoogleGeoLocation } from 'src/Api/GoogleApi';
import GoogleMapMarker from 'src/components/google/GoogleMapMarker';
import { Stakeholders } from 'src/Enums/Stakeholders';
import { IGoogleMapMarker } from 'src/models/IGoogleMapMarker';
import { IStakeholder } from 'src/models/IStakeholder';
import { SelectedDriverAction } from 'src/state/contexts/landing/Actions';
import { FaCreditCard  } from 'react-icons/fa';
import { SaveOrderAction } from 'src/state/contexts/order/Actions';
import { SaveButton } from 'src/components/utils/ActionButtons';
import { SaveStatus } from 'src/Enums/SaveStatus';

export interface IPropsFromDispatch {
    selectedDriverAssigned: (stakeholder: IStakeholder | undefined) => SelectedDriverAction,
    saveOrder: (saved?: boolean) => SaveOrderAction
}

export interface IPropsFromState {
    orderId?: string,
    pickupPlaceName: string,
    dropoffCustomerName: string,
    saveOrderStatus: SaveStatus,
    pickupLocation?: IGoogleGeoLocation,
    selectedDriver?: IStakeholder,
    selectedCustomer?: IStakeholder
}

export interface IOwnState {
    radius: IBaseModel,
    driverLocations: IGoogleMapMarker[]
}

export interface IOwnProps {
    showMap: boolean,
    registrationOn: boolean
}

type AllProps = IPropsFromState & IPropsFromDispatch & IOwnProps;

export default class EmptyStateComponent extends React.Component<AllProps, IOwnState> {

    constructor(props: AllProps) {
        super(props);

        this.state = {
            driverLocations: [],
            radius: {
                id: "4828", 
                name: "3 miles"
            }
        };
    }

    public componentDidMount() {
        if (this.state.driverLocations.length ===0) {
            api.stakeholders(Stakeholders.driver)
                .then(d => this.driversSuccess(d.stakeholders))
        }
    }

    public componentDidUpdate = (prevProps: AllProps, prevState: IOwnState) => {
        if (this.props.selectedDriver !== undefined) {
            if (this.props.selectedDriver?.id !== prevProps.selectedDriver?.id) {
                let newDriverRegistration : IGoogleMapMarker | undefined;

                if (!this.state.driverLocations.map(m => m.driver?.id).includes(this.props.selectedDriver.id))
                {
                    newDriverRegistration = {
                        driver: this.props.selectedDriver,
                        label: this.props.selectedDriver.firstName,
                        selected: true,
                    }
                }

                let marks = this.state.driverLocations.map(l => {
                    return { ...l,  
                        ...{ 
                            selected: l.driver?.id === this.props.selectedDriver?.id 
                        } 
                    }
                })

                if (newDriverRegistration !== undefined) {
                    marks = marks.concat(newDriverRegistration);
                }

                this.setState({ driverLocations: marks })
            }
        }
    }

    public render() {
        const radiusOptions : IBaseModel[] = [
            {id: "4828", name: "3 miles"}, 
            {id: "6437", name: "4 miles"},
            {id: "8046", name:"5 miles"}
        ] 

        return (
            <>
                {
                    this.props.showMap ?
                        <SelectElement 
                            label="Drivers proximity from pickup location"
                            icon="&#xf299;"
                            id="radius"
                            selected={this.state.radius.id}
                            selectorOptions={radiusOptions}
                            onSelectChange={this.handleRadiusChange}
                            />
                    : null
                }
                {
                    !this.props.registrationOn && this.props.selectedDriver !== undefined ?
                        <SaveButton 
                            style={{ float: "right" }} 
                            value={this.props.orderId ? "Update & Checkout" : "Create & Checkout"}
                            icon={<FaCreditCard />}
                            saving={this.props.saveOrderStatus === SaveStatus.Processing} 
                            onSaveClick={() => this.props.saveOrder()} />
                    : null
                }
                {
                    this.props.showMap ?
                        <>
                            <br />
                            <GoogleMapMarker 
                                centerRadius={Number(this.state.radius.id)} 
                                marks={this.state.driverLocations} 
                                pickupLocation={this.props.pickupLocation}
                                selectedCustomer={this.props.selectedCustomer}
                                selectedDriver={this.props.selectedDriver}
                                selectedDriverAssigned={(s: IStakeholder) => this.props.selectedDriverAssigned(s)}
                                excludeCenterMarker={false} 
                            />
                        </>
                    : null
                }
            </>
        );
    }

    private driversSuccess = (drivers: IStakeholder[]) => {
        if (this.props.pickupLocation !== undefined) {

            const pickupLocaton: IGoogleMapMarker = {
                centerMark: this.props.pickupLocation,
                selected: false,
                label: `Pickup from ${this.props.pickupPlaceName}`
            }

            const customerLocation: IGoogleMapMarker = {
                customer: this.props.selectedCustomer,
                selected: false,
                label: `Deliver to ${this.props.dropoffCustomerName}`
            }

            this.setState({ driverLocations: [...drivers.map(driver => ({ 
                    driver: driver,
                    selected: this.props.selectedDriver?.id === driver.id,
                    label: driver.firstName
                } as IGoogleMapMarker)), pickupLocaton, customerLocation]
            })
        }
    }

    private handleRadiusChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
        const radius : IBaseModel = {
            id: event.target.value,
            name: event.target.name
        }
        this.setState({ radius: radius })
    }
}