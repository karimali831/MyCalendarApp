import * as React from 'react';
import { api } from 'src/Api/Api';
import { SelectionRefinement } from 'src/components/SelectionRefinement/SelectionRefinement';
import { ToggleSwitch } from 'src/components/utils/ToggleSwitch';
import { ICustomer } from 'src/models/ICustomer';

export interface IOwnProps {

}

export interface IOwnState {
    filter: string | null,
    customers: ICustomer[],
    loading: boolean
}

export default class NewOrder extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);
        this.state = {
            filter: null,
            customers: [],
            loading: false
        };
    }

    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (prevState.filter !== this.state.filter && this.state.filter != null) {
            this.setState({ loading: true })
            api.customers(this.state.filter)
                .then((c) => this.customersSuccess(c.customers));
            }
    }

    public render() {
        return (
            <div>
                <div className="wrap-login100 p-l-55 p-r-55 p-t-65 p-b-54">
                    <div className="float-right">
                        <span>Customer Registration?</span> <ToggleSwitch name='newOrder' />
                    </div>
                
                    <SelectionRefinement filter={this.state.filter} onChange={(e) => this.keywordsChanged(e)} />
                    {
                        !this.state.loading ?
                            this.state.customers.length > 0 ?
                            <>
                                {
                                    this.state.customers.map((s, idx) => 
                                        <span key={idx} className="badge badge-primary">{s.firstName} {s.lastName} @ {s.address1} {s.postcode}</span>
                                    )
                                }
                            </> 
                            : (this.state.filter !== null && this.state.filter !== "" ? <span className="badge badge-danger">No customers found with filtered search</span> : null)
                        : <div id="cover-spin" />
                    }
                    <hr />
                    <span className="login100-form-title p-b-49">
                        Customer Order
                    </span>
                    <div className="wrap-input100 validate-input m-b-23">
                        <span className="label-input100">Start Date</span>
                        <input type="text" name="" defaultValue="" className="form input100" />
                        <span className="focus-input100" data-symbol="&#xf190;" />
                    </div>
                </div>
            </div>
        );
    }

    private keywordsChanged = (filter: string) => {
        this.setState({ filter: filter })
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