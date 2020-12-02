import * as React from "react";
import { api, ICustomerRegisterResponse } from 'src/Api/Api';
import { IAddress } from 'src/models/IAddressFinder';
import { ICustomer } from 'src/models/ICustomer';
import { Load } from '../../base/Loader';

export interface IOwnProps {
    onRegistrationChange: (cusotmer: ICustomer) => void
}

export interface IOwnState {
    customer: ICustomer,
    postcodeAddresses: any[],
    loadingAddresses: boolean,
    apiAddresses: boolean
}

export default class CustomerRegistration extends React.Component<IOwnProps, IOwnState> {
    constructor(props: any) {
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
            apiAddresses: false,
            postcodeAddresses: [],
            loadingAddresses: false
        };
        this.handleChange = this.handleChange.bind(this);
        this.populateAddress = this.populateAddress.bind(this);
    }
  

    public handleChange(event: any): void {
        this.setState({
            customer: { ...this.state.customer,    
                [event.target.name]: event.target.value    
            }
        })
    }

    public handlePostcodeOnChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.setState({
            customer: { ...this.state.customer,    
                postcode: e.target.value
            }
        })

        if (this.valid_postcode(e.target.value)) {
            if (this.state.apiAddresses) {
                this._addressFromPostcode(e.target.value);
            }
        }
    }

    public populateAddress(event: any): void {
        const data: string = event.target.value;
        const splitData: string[] = data.split(",");

        this.setState(prevState => ({
            customer: { ...prevState.customer,    
                address1: splitData[0],
                address2: splitData[1],
                address3: splitData[2],
                town: splitData[5],
                county: splitData[6]
            }
        }))

        this.renderAddressDetails();
    }

    public renderPostcodeAddresses(): JSX.Element | null {
        if (this.state.loadingAddresses) {
            if (this.state.postcodeAddresses.length > 0) {
                return (
                    <div className="wrap-input100 m-b-23">
                        <span className="label-input100">Address</span>
                        <select className="form input100" name="postcodeAddresses" onChange={this.populateAddress} required={true} >
                            {this.renderAddressOptions()}
                        </select>
                        <span className="focus-input100" data-symbol="&#xf175;" />
                    </div>
                );
            } 
            else {
                return <Load />;
            }
        }
        return null;
    }

    public renderAddressDetails(): JSX.Element[] | JSX.Element {
        if ((this.state.customer.address1 && this.state.customer.county) || !this.state.apiAddresses) {
            const changedInputs: IAddress = {
                id: [
                  "address1",
                  "address2",
                  "address3",
                  "town",
                  "county",
                  "country"],
                label: [
                  "Address Line 1",
                  "Address Line 2",
                  "Address Line 3",
                  "Town",
                  "County",
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

    public renderAddressOptions(): JSX.Element[] {
        return this.state.postcodeAddresses.map(
            (address: string, index: number) => {
                return (
                    <option key={index} value={address}>
                      {address} 
                    </option>
                );
            }
        );
    }
  
    public handleRegistrationSubmit = (event: any): Promise<boolean> | false => {
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

    public customerRegistrationSuccess = (customerRegistration: ICustomerRegisterResponse) => {
        if (customerRegistration.customer != null) {
            this.props.onRegistrationChange(customerRegistration.customer);
        }
        else{
            alert(customerRegistration.message);
        }
    }

    public render(): JSX.Element {
        return (
            <form onSubmit={this.handleRegistrationSubmit}>
              <span className="login100-form-title p-b-49">
                  Customer Registration
              </span>
              <div className="wrap-input100 m-b-23">
                  <span className="label-input100">First Name</span>
                  <input type="text" name="firstName" defaultValue="" className="form input100" onChange={this.handleChange} required={true}  />
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
              <div className="wrap-input100 m-b-23">
                  <span className="label-input100">Postcode</span>
                  <input type="text"
                      name="postcode"
                      defaultValue={this.state.customer.postcode}
                      placeholder="e.g SE4 2JF"
                      className="form input100"
                      onChange={this.handlePostcodeOnChange} required={true} />
                  <span className="focus-input100" data-symbol="&#xf1ab;" />
              </div>
              {this.state.apiAddresses ? this.renderPostcodeAddresses() : this.populateAddress}
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
        );
    }

    private valid_postcode(postcode: string) {
        postcode = postcode.replace(/\s/g, "");
        const regex = new RegExp('([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]?))))\s?[0-9][A-Za-z]{2})');
        return regex.test(postcode);
    }

    private _addressFromPostcode(postcode: string): void {
        const convertedPostcode: string = this._removeWhitespace(postcode);
        const API_KEY = "TAbHcgudWkeBaSM31zj-Mg29276";
        const API_URL = `https://api.getaddress.io/find/${convertedPostcode}?api-key=${API_KEY}`;
        let displayText = "Select an address";

        this.setState({ loadingAddresses: true });

        fetch(API_URL)
          .then(response => {
            if (!response.ok) {
              displayText =
                "Something went wrong :( Check your postcode or try again later.";
              // throw console.log(response);
            }
            return response.json();
          })
          .then(data =>
            this.setState({ postcodeAddresses: [displayText, ...data.addresses] })
          );
    }

    private _removeWhitespace(postcode: string): string {
        return postcode.replace(/\s/g, "");
    }
}