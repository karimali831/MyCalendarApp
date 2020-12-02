import * as React from 'react';
import { api } from 'src/Api/Api';
import { Load } from 'src/components/base/Loader';
import { SelectionRefinement } from 'src/components/SelectionRefinement/SelectionRefinement';
import { ToggleSwitch } from 'src/components/utils/ToggleSwitch';
import { ICustomer } from 'src/models/ICustomer';
import CustomerRegistration from './CustomerRegistration';

export interface IOwnProps {

}

export interface IOwnState {
    filter: string,
    customers: ICustomer[],
    registrationOn: boolean,
    selectedCustomer: ICustomer | null
    loading: boolean
}

export default class NewOrder extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);
        this.state = {
            filter: "",
            customers: [],
            registrationOn: false,
            selectedCustomer: null,
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
                        New Customer/Order
                    </span>
                    <SelectionRefinement filter={this.state.filter} onChange={(f) => this.keywordsChanged(f)} onLoading={(l) => this.onLoadingChanged(l)} />
                    <div>
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
                    </div>
                    <ToggleSwitch id="custReg" name='New Customer' onChange={c => this.registrationOn(c)} checked={this.state.registrationOn} />
                    <hr />
                    {
                        this.state.registrationOn ? <CustomerRegistration onRegistrationChange={c => this.registrationChange(c)} /> :
                        <>
                            {
                                this.state.selectedCustomer != null ?
                                <>
                                    <div>
                                        {this.customerLabel(this.state.selectedCustomer)}
                                        <span className="login100-form-title p-b-49">Customer Order</span>
                                    </div>
        
                                    <div className="wrap-input100 validate-input m-b-23">
                                        <span className="label-input100">Start Date</span>
                                        <input type="text" name="" defaultValue="" className="form input100" />
                                        <span className="focus-input100" data-symbol="&#xf1c3;" />
                                    </div>
                                </>
                                : null
                            }
                        </>
                    }
                </div>
            </div>
        );
    }    

    private customerLabel = (c: ICustomer) => 
        <span className="badge badge-primary">
            {c.firstName} {c.lastName} @ {c.address1} {c.postcode}
        </span>
    

    private selectedCustomer = (customer: ICustomer) => {
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
            selectedCustomer: null,
            filter: "",
            customers: []
        })
    }

    private registrationChange = (customer: ICustomer) => {
        this.selectedCustomer(customer)
    }

    private keywordsChanged = (filter: string) => {
        this.setState({ filter: filter })
    }

    private onLoadingChanged = (loading: boolean) => {
        this.setState({ loading: loading })
    }

    private customersSuccess = (customers: ICustomer[]) => {
        this.setState({ ...this.state,
            ...{ 
                loading: false,
                customers: customers
            }
        }) 
      }
}