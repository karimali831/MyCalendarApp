import { SelectionRefinement, ToggleSwitch, Variant } from '@appology/react-components';
import * as React from 'react';
import { Stakeholders } from 'src/Enums/Stakeholders';
import {  IStakeholder, IStakeholderSearch } from 'src/models/IStakeholder';
import { SelectedCustomerAction, SelectedDriverAction, ToggleAlertAction } from 'src/state/contexts/landing/Actions';
import DriverLocatorConnected from '../step4-driver/DriverLocatorConnected';
import RegistrationForm from './RegistrationForm';


export interface IPropsFromDispatch {
    searchStakeholder: (filter: string, stakeholderId: Stakeholders) => void,
    selectedCustomerChange: (stakeholder: IStakeholder | undefined) => SelectedCustomerAction,
    selectedDriverChange: (stakeholder: IStakeholder | undefined) => SelectedDriverAction,
    handleAlert: (text: string, variant?: Variant, timeout?: number) => ToggleAlertAction
}

export interface IPropsFromState {
    activeStep: number,
    filter: string,
    loading: boolean,
    stakeholders: IStakeholderSearch[],
    selectedDriver: IStakeholder | undefined
}

export interface IOwnProps {}

export interface IOwnState {
    customerRegistrationOn: boolean,
    driverRegistrationOn: boolean,
    showDriverMap: boolean
}

type AllProps = IPropsFromState & IPropsFromDispatch & IOwnProps;

export default class Registration extends React.Component<AllProps, IOwnState> {

    constructor(props: AllProps) {
        super(props);

        this.state = {
            driverRegistrationOn: false,
            customerRegistrationOn: false,
            showDriverMap: this.props.selectedDriver !== undefined ? true : false,
        };
    }

    public render() {
        return (
            <>
                {
                    this.props.activeStep === 0 ?
                    <>
                        <SelectionRefinement<IStakeholderSearch>
                            label="Customer search" 
                            focus={true}
                            placeholder="Search by customer info..." 
                            filter={this.props.filter} 
                            onChange={(f) => this.props.searchStakeholder(f, Stakeholders.customer)} 
                            loading={this.props.loading}
                            setFilterToItemName={true}
                            itemSelected={(i) => this.selectedCustomer(i.stakeholder)}
                            filteredResults={this.props.stakeholders} 
                        />
                        <ToggleSwitch id="customerReg" name='New customer' onChange={c => this.customerRegistrationOn(c)} checked={this.state.customerRegistrationOn} />
                        {
                            this.state.customerRegistrationOn ? 
                                <RegistrationForm
                                    stakeholderId={Stakeholders.customer} 
                                    onRegistrationChange={c => this.customerRegistrationChange(c)} /> 
                                : null
                        }
                    </> 
                    : this.props.activeStep === 3 ?
                    <>
                        {
                            !this.state.showDriverMap ?
                                <>
                                    <SelectionRefinement<IStakeholderSearch>
                                        label="Driver search" 
                                        focus={true}
                                        placeholder="Search by driver info..." 
                                        filter={this.props.filter} 
                                        onChange={(f) => this.props.searchStakeholder(f, Stakeholders.driver)} 
                                        loading={this.props.loading}
                                        setFilterToItemName={false}
                                        itemSelected={(i) => this.selectedDriver(i.stakeholder)}
                                        filteredResults={this.props.stakeholders} 
                                    />
                                    <ToggleSwitch inline={true} id="driverReg" name='New driver' onChange={c => this.driverRegistrationOn(c)} checked={this.state.driverRegistrationOn} />
                                </>
                            : null
                        }        
                        <ToggleSwitch inline={true} id="showMap" name='Show map' disabled={this.state.driverRegistrationOn} onChange={c => this.showDriverMap(c)} checked={this.state.showDriverMap} />
                        {
                            this.state.driverRegistrationOn && !this.state.showDriverMap ? 
                                <RegistrationForm
                                    stakeholderId={Stakeholders.driver} 
                                    onRegistrationChange={c => this.driverRegistrationChange(c)} /> 
                                : null
                        }
                        <hr />
                        <DriverLocatorConnected 
                            showMap={this.state.showDriverMap} 
                            registrationOn={this.state.driverRegistrationOn} 
                        />
                    </>
                    : null
                }
            </>
        );
    }


    /* customer */
    private selectedCustomer = (stakeholder: IStakeholder) => {
        this.props.selectedCustomerChange(stakeholder);
        this.setState({ customerRegistrationOn: false })

    }

    private customerRegistrationOn = (checked: boolean) => {
        this.setState({ customerRegistrationOn: checked })
    }

    private customerRegistrationChange = (stakeholder: IStakeholderSearch) => {
        this.props.handleAlert("Customer registration successful")
        this.selectedCustomer(stakeholder.stakeholder)
    }

    /* driver */
    private selectedDriver = (stakeholder: IStakeholder) => {
        this.props.selectedDriverChange(stakeholder);

        this.setState({ 
            driverRegistrationOn: false,
            showDriverMap: true
        })
    }

    private driverRegistrationOn = (checked: boolean) => {
        this.setState({ driverRegistrationOn: checked })
    }

    private showDriverMap = (checked: boolean) => {
        this.setState({ showDriverMap: checked })
    }

    private driverRegistrationChange = (stakeholder: IStakeholderSearch) => {
        this.props.handleAlert("Driver registration successful");
        this.selectedDriver(stakeholder.stakeholder)
    }
}