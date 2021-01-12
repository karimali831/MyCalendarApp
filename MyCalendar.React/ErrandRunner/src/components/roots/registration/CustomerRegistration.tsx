import * as React from "react";
import { api, ICustomerRegisterResponse } from 'src/Api/Api';
import { customerSearchTxt } from 'src/components/utils/Utils';
import { IAddress, IAddressLabel, IAddressPrediction, IAddressSearch } from 'src/models/IAddressFinder';
import { ICustomer, ICustomerSearch } from 'src/models/ICustomer';
import { SelectionRefinement } from '@appology/react-components';
import { addressApi } from "src/Api/AddressApi";

export interface IOwnProps {
    onRegistrationChange: (customer: ICustomerSearch) => void
}

export interface IOwnState {
    customer: ICustomer,
    addresses: IAddressSearch[],
    loadingAddresses: boolean,
    apiAddresses: boolean,
    filter: string
}

export default class CustomerRegistration extends React.Component<IOwnProps, IOwnState> {
    constructor(props: IOwnProps) {
        super(props);
        this.state = {
            customer: {
                custId: "",
                firstName: "",
                lastName: "",
                postcode: "",
                address1: "",
                address2: "",
                address3: "",
                town: "",
                county: "",
                contactNo1: "",
                contactNo2: "",
                country: "United Kingdom"
              },
            filter: "",
            apiAddresses: true,
            addresses: [],
            loadingAddresses: false
        };
        this.handleChange = this.handleChange.bind(this);
    }
  
    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (prevState.filter !== this.state.filter && this.state.filter !== "") {
            addressApi.addressSearch(this.state.filter)
                .then((c) => this.addressSearchSuccess(c.predictions));
        }
    }

    public render(): JSX.Element {
        return (
            <div className="customer-registration">
                <form onSubmit={this.handleRegistrationSubmit}>
                    <div className="wrap-input100 m-b-23">
                        <span className="label-input100">First Name</span>
                        <input autoFocus={true} type="text" name="firstName" defaultValue="" className="form input100" onChange={this.handleChange} required={true}  />
                        <span className="focus-input100" data-symbol="&#xf106;" />
                    </div>
                    <div className="wrap-input100 m-b-23">
                        <span className="label-input100">Surname</span>
                        <input type="text" name="lastName" defaultValue="" className="form input100" onChange={this.handleChange} required={true}  />
                        <span className="focus-input100" data-symbol="&#xf106;" />
                    </div>
                    <div className="wrap-input100 m-b-23">
                        <span className="label-input100">Email</span>
                        <input type="text" name="email" defaultValue="" className="form input100" onChange={this.handleChange}  />
                        <span className="focus-input100" data-symbol="&#xf15a;" />
                    </div>

                    <SelectionRefinement<IAddressSearch>
                            label="Address Search" 
                            placeholder="Enter address..." 
                            filter={this.state.filter} 
                            focus={false}
                            onChange={(f) => this.handleSearchAddressChange(f)} 
                            loading={this.state.loadingAddresses}
                            setFilterToItemName={true}
                            itemSelected={(i) => this.addressSelected(i)}
                            filteredResults={this.state.addresses} 
                        />

                    {this.renderAddressDetails()}
                    <div className="wrap-input100 m-b-23">
                        <span className="label-input100">Contact Number</span>
                        <input type="text" name="contactNo1" defaultValue="" className="form input100" onChange={this.handleChange} required={true}  />
                        <span className="focus-input100" data-symbol="&#xf2c8;" />
                    </div>
                    <div className="wrap-input100 m-b-23">
                        <span className="label-input100">Altnernate Contact</span>
                        <input type="text" name="contactNo2" defaultValue="" className="form input100" onChange={this.handleChange}  />
                        <span className="focus-input100" data-symbol="&#xf2c8;" />
                    </div>
                    <button type="submit" className="btn btn-primary">Register</button>
                </form>
            </div>
        );
    }

    private addressSelected = (address: IAddressSearch) => {
        addressApi.getAddress(address.name, address.id)
            .then(a => this.addressSelectedSuccess(a))
    }

    private addressSelectedSuccess = (address: IAddress[]) => {

        if (address.length === 1) {
            this.setState(prevState => ({
                customer: { ...prevState.customer,    
                    address1: address[0].addressline1,
                    town: address[0].posttown,
                    county: address[0].county,
                    postcode: address[0].postcode
                }
            }))
        }

        this.renderAddressDetails();
    }

    private handleChange(event: React.ChangeEvent<HTMLInputElement>): void {
        this.setState({
            customer: { ...this.state.customer,    
                [event.target.name]: event.target.value    
            }
        })
    }

    private handleSearchAddressChange(filter: string) {
        this.setState({ 
            filter: filter, 
            loadingAddresses: true 
        })
    }


    private renderAddressDetails(): JSX.Element[] | JSX.Element {
        if (this.state.customer.address1 || !this.state.apiAddresses) {
            const changedInputs: IAddressLabel = {
                id: [
                  "address1",
                  "address2",
                  "address3",
                  "town",
                  "county",
                  "postcode",
                  "country"],
                label: [
                  "Address Line 1",
                  "Address Line 2",
                  "Address Line 3",
                  "Town",
                  "County",
                  "Postcode",
                  "Country"],
            };

          return changedInputs.id.map((input: string, idx) => {
              return (
                <div key={input} className="wrap-input100 m-b-23">
                    <span className="label-input100">{changedInputs.label[idx]}</span>
                    <input value={this.state.customer[input]} onChange={this.handleChange} type="text" name={input} className="form input100" required={input === "address1" ? true : false} disabled={(input === "country")? true : false} />
                    {
                      (() => {
                          switch(input) {
                              case "address1":
                                  return <span className="focus-input100" data-symbol="&#xf31f;" />;
                              case "address2":
                                  return <span className="focus-input100" data-symbol="&#xf31f;" />;
                              case "address3":
                                  return <span className="focus-input100" data-symbol="&#xf31f;" />;
                              case "town":
                                  return <span className="focus-input100" data-symbol="&#xf132;" />;
                              case "county":
                                  return <span className="focus-input100" data-symbol="&#xf133;" />;
                              case "postcode":
                                    return <span className="focus-input100" data-symbol="&#xf299;" />;
                              case "country":
                                  return <span className="focus-input100" data-symbol="&#xf171;" />;
                              default:
                                  return <span className="focus-input100" />;
                          }
                      })()
                    }
                </div>
              );
          });
        } 
        else {
            return <div />;
        }
    }

    private handleRegistrationSubmit = (event: any): Promise<boolean> | false => {
        event.preventDefault();
        if (this.state.customer !== null) {
            if (this.state.customer.address1 === "") {
                alert ("Address must be selected");
                return false;
            }
            else {
                api.registerCustomer(this.state.customer as ICustomer)
                    .then(c => this.customerRegistrationSuccess(c));
            }
        }
        return false;
    }

    private customerRegistrationSuccess = (c: ICustomerRegisterResponse) => {
        if (c.customer != null) {
            const customerSearch: ICustomerSearch = {
                id: c.customer.custId,
                name: customerSearchTxt(c.customer),
                addressLine1: c.customer.address1,
                postCode: c.customer.postcode
            }

            this.props.onRegistrationChange(customerSearch);
        }
        else{
            alert(c.message);
        }
    }


    private addressSearchSuccess = (addresses:  IAddressPrediction[]) => {
        this.setState({ ...this.state,
            ...{ 
                loadingAddresses: false,
                addresses: addresses.map(address =>({ 
                    id: address.refs, 
                    name: address.prediction
                } as IAddressSearch))
            }
        }) 
        
    }
}