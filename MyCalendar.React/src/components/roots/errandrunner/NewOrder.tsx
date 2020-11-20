import * as React from 'react';
import { api } from 'src/Api/Api';
import { Load } from 'src/components/base/Loader';
import { SelectionRefinement } from 'src/components/SelectionRefinement/SelectionRefinement';
import { ToggleSwitch } from 'src/components/utils/ToggleSwitch';
import { ICustomer } from 'src/models/ICustomer';

export interface IOwnProps {

}

export interface IOwnState {
    filter: string | null,
    customers: ICustomer[],
    registrationOn: boolean,
    selectedCustomer: ICustomer | null
    loading: boolean
}

export default class NewOrder extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);
        this.state = {
            filter: null,
            customers: [],
            registrationOn: false,
            selectedCustomer: null,
            loading: false
        };
    }

    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (prevState.filter !== this.state.filter && this.state.filter != null) {
            api.customers(this.state.filter)
                .then((c) => this.customersSuccess(c.customers));
        }
    }

    public render() {
        return (
            <div>
                <div className="wrap-login100 p-l-55 p-r-55 p-t-65 p-b-54">

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
                                    this.state.filter !== null && this.state.filter !== "" ? 
                                        <div>
                                            <span className="badge badge-danger">No customers found with filtered search</span> 
                                            <hr />
                                        </div>
                                    : null)
                            : <Load />
                        }
                    </div>
        
                    <ToggleSwitch id="custReg" name='Customer Registration?' onChange={c => this.registrationOn(c)} checked={this.state.registrationOn} />
                    <hr />
                    {

                        this.state.registrationOn ?
                        <>
                            <span className="login100-form-title p-b-49">
                                Customer Registration
                            </span>
                            <div className="wrap-input100 validate-input m-b-23">
                                <span className="label-input100">First Name</span>
                                <input type="text" name="firstName" defaultValue="" className="form input100" />
                                <span className="focus-input100" data-symbol="&#xf106;" />
                            </div>
                            <div className="wrap-input100 validate-input m-b-23">
                                <span className="label-input100">Surname</span>
                                <input type="text" name="lastName" defaultValue="" className="form input100" />
                                <span className="focus-input100" data-symbol="&#xf106;" />
                            </div>
                            <div className="wrap-input100 validate-input m-b-23">
                                <span className="label-input100">Email</span>
                                <input type="text" name="email" defaultValue="" className="form input100" />
                                <span className="focus-input100" data-symbol="&#xf15a;" />
                            </div>
                            <div className="wrap-input100 validate-input m-b-23">
                                <span className="label-input100">Address Line 1</span>
                                <input type="text" name="address1" defaultValue="" className="form input100" />
                                <span className="focus-input100" data-symbol="&#xf175;" />
                            </div>
                            <div className="wrap-input100 validate-input m-b-23">
                                <span className="label-input100">Address Line 2</span>
                                <input type="text" name="address2" defaultValue="" className="form input100" />
                                <span className="focus-input100" data-symbol="&#xf175;" />
                            </div>
                            <div className="wrap-input100 validate-input m-b-23">
                                <span className="label-input100">Town</span>
                                <input type="text" name="town" defaultValue="" className="form input100" />
                                <span className="focus-input100" data-symbol="&#xf133;" />
                            </div>
                            <div className="wrap-input100 validate-input m-b-23">
                                <span className="label-input100">Post Code</span>
                                <input type="text" name="postcode" defaultValue="" className="form input100" />
                                <span className="focus-input100" data-symbol="&#xf1ab;" />
                            </div>
                            <div className="wrap-input100 validate-input m-b-23">
                                <span className="label-input100">Contact Number</span>
                                <input type="text" name="contactNo1" defaultValue="" className="form input100" />
                                <span className="focus-input100" data-symbol="&#xf2c8;" />
                            </div>
                            <div className="wrap-input100 validate-input m-b-23">
                                <span className="label-input100">Altnernate Contact</span>
                                <input type="text" name="contactNo2" defaultValue="" className="form input100" />
                                <span className="focus-input100" data-symbol="&#xf2c8;" />
                            </div>
                        </>
                        :
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
            filter: null,
            registrationOn: false
        })
    }

    private registrationOn = (checked: boolean) => {
        this.setState({ 
            registrationOn: checked,
            selectedCustomer: null
        })
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