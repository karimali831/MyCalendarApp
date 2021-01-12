import { SelectionRefinement, ToggleSwitch, Modal, Variant, SelectElement } from '@appology/react-components';
import IBaseModel from '@appology/react-components/src/SelectionRefinement/IBaseModel';
import * as React from 'react';
import { api } from 'src/Api/Api';
import { googleApi, IGoogleDistanceMatrixRows, IGoogleGeoLocation } from 'src/Api/GoogleApi';
import { customerSearchTxt } from 'src/components/utils/Utils';
import { ICustomer, ICustomerSearch } from 'src/models/ICustomer';
import { IGoogleAutoCompleteSearch } from 'src/models/IGoogleAutoComplete';
import { ITripOverview } from 'src/models/TripOverview';
import CustomerRegistration from '../registration/CustomerRegistration';
import { OrderDetails } from './OrderDetails';
import PickupLocation from '../storelocator/PickupLocation';
import { Navigator } from '../../menu/Navigator'
import { SidebarMenu } from 'src/components/menu/SidebarMenu';
import { IDefaultConfig } from 'src/models/IDefaultConfig';

interface IOwnProps {

}

interface IOwnState {
    filter: string,
    customers: ICustomerSearch[],
    registrationOn: boolean,
    alert: boolean,
    alertMsg: string,
    services: IBaseModel[], 
    selectedService?: IBaseModel,
    selectedCustomer?: ICustomerSearch
    pickupLocation: string,
    pickupId: string,
    customerLocation: IGoogleGeoLocation,
    pinSidebar: boolean,
    distance: string,
    duration: string,
    step: number,
    config: IDefaultConfig,
    loading: boolean
}

export class NewOrder extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);
        this.state = {
            filter: "",
            customers: [],
            registrationOn: false,
            alert: false,
            alertMsg: "",
            services: [],
            selectedService: undefined,
            selectedCustomer: undefined,
            pickupLocation: "",
            pinSidebar: false,
            distance: "", 
            duration: "",
            pickupId: "",
            customerLocation: {
                lat: "",
                lng: ""
            },
            step: 0,
            config: this.defaultConfig(),
            loading: false
        };
    }

    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (prevState.filter !== this.state.filter && this.state.filter !== "") {
            api.customers(this.state.filter)
                .then((c) => this.customersSuccess(c.customers));
        }

        if (prevState.pickupId !== this.state.pickupId && this.state.pickupId !== "") {
            googleApi.distanceMatrix(this.state.pickupId, this.state.customerLocation)
                .then(s => this.distanceMatrixSuccess(s.rows))
        }

        if (JSON.stringify(this.state.config) !== JSON.stringify(this.state.config)) {
            this.updateConfigChange(this.state.config)
        }
    }

    public render() {
        let tripOverview : ITripOverview | undefined;
 
        if (this.state.selectedCustomer !== undefined && this.state.pickupId !== "")  {
            tripOverview = {
                pickupId: this.state.pickupId,
                pickupLocation: this.state.pickupLocation,
                customerLocation: this.state.customerLocation,
                distance: this.state.distance,
                duration: this.state.duration,
                dropOffAddress: this.state.selectedCustomer.addressLine1,
                dropOffPostCode: this.state.selectedCustomer.postCode
            }
        }
        
        return (
            <div>
                <SidebarMenu 
                    initialState={this.resetConfig}
                    pinSidebar={this.state.pinSidebar && this.state.step === 2} 
                    config={this.state.config}
                    updateConfig={this.updateConfigChange}
                />  
                <div className={this.state.pinSidebar && this.state.step === 2 ? "sidebar-margin" : ""}>
                    <div className="wrap-login100 p-l-55 p-r-55 p-t-65 p-b-54">
                        <Navigator 
                            customerName={this.state.selectedCustomer?.name.split('@')[0]}
                            pickupName={this.state.pickupLocation?.split(' ')[0]}
                            service={this.state.selectedService?.name}
                            activeStep={this.state.step} 
                            setActiveStep={(step: number) => this.setStep(step)} />
                        {
                            this.state.alert ?
                                <Modal show={true} text={this.state.alertMsg} variant={Variant.Success} />
                            : null
                        }
                        {
                            this.state.step === 0 ? <>
                                <SelectionRefinement<ICustomerSearch>
                                    label="Customer Search" 
                                    focus={true}
                                    placeholder="Search by customer info..." 
                                    filter={this.state.filter} 
                                    onChange={(f) => this.keywordsChanged(f)} 
                                    loading={this.state.loading}
                                    setFilterToItemName={true}
                                    itemSelected={(i) => this.selectedCustomer(i)}
                                    filteredResults={this.state.customers} 
                                />
                                <ToggleSwitch id="custReg" name='New Customer' onChange={c => this.registrationOn(c)} checked={this.state.registrationOn} />
                                {this.state.registrationOn ? <CustomerRegistration onRegistrationChange={c => this.registrationChange(c)} /> : null}
                            </>
                            : this.state.step === 1 && this.state.selectedCustomer !== undefined ?
                                <>
                                    <SelectElement 
                                        label="Services"
                                        selectorName="Select Service"
                                        focus={true}
                                        icon="&#xf108;"
                                        id="service"
                                        selected={this.state.selectedService?.id.toString()}
                                        selectorOptions={this.state.services}
                                        onSelectChange={this.handleServiceChange}
                                    />
                                    <PickupLocation 
                                        onStoreChange={this.storePickupChange}
                                        addressLine1={this.state.selectedCustomer.addressLine1} 
                                        postCode={this.state.selectedCustomer.postCode} 
                                    />
                                </>
                                : this.state.step === 2 && tripOverview !== undefined ?
                                    <OrderDetails 
                                        trip={tripOverview} 
                                        pinSidebar={this.state.pinSidebar}
                                        configChange={this.configChange}
                                        config={this.state.config} />
                            : null
                        }
                    </div>
                </div>
            </div>
        );
    }    

    private configChange = () => {
        this.setState({ pinSidebar: !this.state.pinSidebar })
    }

    private defaultConfig = () => {
        const config : IDefaultConfig = {     
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
            driverFee: 0.3
        }

        return config;
    }

    private resetConfig = () => {
        this.updateConfigChange(this.defaultConfig());
    }

    private updateConfigChange = (config: IDefaultConfig) => {
        this.setState({ config: config })
    }

    private handleServiceChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const service : IBaseModel = {
            id: e.target.value,
            name: e.target.name
        }

        this.setStep(this.state.pickupLocation !== "" ? 2 : 1);
        this.setState({ selectedService: service })
    }

    private servicesSuccess = (services: IBaseModel[]) => {
        this.setState({ 
            services: services
        })
    }

    private selectedCustomer = (customer: ICustomerSearch) => {
        this.setStep(1);

        this.setState({ 
            selectedCustomer: customer, 
            customers: [], 
            filter: "",
            registrationOn: false
        })

        api.services()
            .then(s => this.servicesSuccess(s.services))

    }

    private setStep = (step: number) => {
        this.setState({ step: step })
    }

    private registrationOn = (checked: boolean) => {
        this.setState({ 
            registrationOn: checked,
            selectedCustomer: undefined,
            filter: "",
            customers: []
        })
    }

    private registrationChange = (customer: ICustomerSearch) => {
        this.showAlert(`Registration successful`)
        this.selectedCustomer(customer)
    }

    private showAlert = (msg: string) => {
        this.setState({
            alert: true,
            alertMsg: msg
        })
    }

    private storePickupChange = (store: IGoogleAutoCompleteSearch, customerLocation: IGoogleGeoLocation) => {
        this.setStep(this.state.selectedService !== undefined ? 2 : 1);

        this.setState({ 
            pickupLocation: store.name,
            pickupId: store.id,
            customerLocation: customerLocation
        })
    }

    private distanceMatrixSuccess = (distanceMatrix: IGoogleDistanceMatrixRows[]) => {
        if (distanceMatrix.length === 1 && distanceMatrix[0].elements.length === 1) {
            const matrix = distanceMatrix[0].elements[0];

            this.setState({ 
                distance: matrix.distance.text,
                duration: matrix.duration.text
            })
        }
    }

    private keywordsChanged = (filter: string) => {
        this.setState({ 
            loading: true, 
            filter: filter 
        })
    }

    private customersSuccess = (customers: ICustomer[]) => {
        this.setState({ ...this.state,
            ...{ 
                loading: false,
                customers: customers.map(person =>({ 
                    id: person.custId, 
                    name: customerSearchTxt(person),
                    addressLine1: person.address1,
                    postCode: person.postcode
                } as ICustomerSearch))
            }
        }) 
    }
}