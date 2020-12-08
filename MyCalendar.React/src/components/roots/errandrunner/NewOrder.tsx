import * as React from 'react';
import { api } from 'src/Api/Api';
import { SelectionRefinement } from 'src/components/SelectionRefinement/SelectionRefinement';
import { ToggleSwitch } from 'src/components/utils/ToggleSwitch';
import { customerSearchTxt } from 'src/components/utils/Utils';
import { ICustomer, ICustomerSearch } from 'src/models/ICustomer';
import { IGoogleAutoCompleteSearch } from 'src/models/IGoogleAutoComplete';
import CustomerRegistration from './CustomerRegistration';
import PickupLocation from './PickupLocation';

export interface IOwnProps {

}

export interface IOwnState {
    filter: string,
    customers: ICustomerSearch[],
    registrationOn: boolean,
    selectedCustomer?: ICustomerSearch
    pickupLocation: string,
    loading: boolean
}

export default class NewOrder extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);
        this.state = {
            filter: "",
            customers: [],
            registrationOn: false,
            selectedCustomer: undefined,
            pickupLocation: "",
            loading: false
        };
    }

    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (prevState.filter !== this.state.filter && this.state.filter !== "") {
            api.customers(this.state.filter)
                .then((c) => this.customersSuccess(c.customers));
        }
    }

    public render() {
        return (
            <div>
                <div className="wrap-login100 p-l-55 p-r-55 p-t-65 p-b-54">
                    <span className="login100-form-title p-b-49">
                        Customer Search
                    </span>
                    <SelectionRefinement<ICustomerSearch>
                        label="Customer Search" 
                        placeholder="Search by customer info..." 
                        filter={this.state.filter} 
                        onChange={(f) => this.keywordsChanged(f)} 
                        loading={this.state.loading}
                        setFilterToItemName={true}
                        itemSelected={(i) => this.selectedCustomer(i)}
                        filteredResults={this.state.customers} />
                    {/* <div>
                        {
                            !this.state.loading ?
                                this.state.customers.length > 0 ?
                                <>
                                    {
                                        this.state.customers.map((s, idx) => 
                                            <div key={idx} >
                                                {this.customerLabel(s)}
                                                <span className="badge badge-success" onClick={() => this.selectedCustomer(s)}>New Order</span>
                                                <hr />
                                            </div>
                                        )
                                    }
                                </> 
                                : (
                                    this.state.filter !== "" ? 
                                        <div>
                                            <span className="badge badge-danger">No customers found with filtered search</span> 
                                            <hr />
                                        </div>
                                    : null)
                            : <Load />
                        }
                    </div> */}
                    <ToggleSwitch id="custReg" name='New Customer' onChange={c => this.registrationOn(c)} checked={this.state.registrationOn} />
                    <hr />
                    {
                        this.state.registrationOn ? <CustomerRegistration onRegistrationChange={c => this.registrationChange(c)} /> :
                        <>
                            {
                                this.state.selectedCustomer != null ?
                                <>
                                    <div>
                                        <span className="badge badge-primary">
                                            {this.state.selectedCustomer.name}
                                        </span>
                                        <span className="login100-form-title p-b-49">Customer Order</span>
                                    </div>
                                    <PickupLocation onStoreChange={this.storePickupChange} />
                                    {
                                        this.state.pickupLocation ? 
                                            <>
                                                <div>
                                                    {this.state.pickupLocation}
                                                    <iframe
                                                        width="600"
                                                        height="450"
                                                        frameBorder="0" style={{"border": "0"}}
                                                        src={`https://www.google.com/maps/embed/v1/place?key=${api.googleApiKey2}&q=${this.state.pickupLocation}`} allowFullScreen={true} />
                                                    
                                                    <span>Directions</span>
                                                </div>
                                            </>
                                
                                        : null
                                    }
                                </>
                                : null
                            }
                        </>
                    }
                </div>
            </div>
        );
    }    

    private selectedCustomer = (customer: ICustomerSearch) => {
        this.setState({ 
            selectedCustomer: customer, 
            customers: [], 
            filter: "",
            registrationOn: false
        })
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
        this.selectedCustomer(customer)
    }

    private storePickupChange = (store: IGoogleAutoCompleteSearch) => {
        this.setState({ pickupLocation: store.name })
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
                    name: customerSearchTxt(person)
                } as ICustomerSearch))
            }
        }) 
    }
}