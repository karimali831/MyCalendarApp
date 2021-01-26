import * as React from "react";
import { api, IStakeholderRegisterResponse } from 'src/Api/Api';
import { stakeholderSearchTxt } from 'src/components/utils/Utils';
import { IAddress, IAddressLabel, IAddressPrediction, IAddressSearch } from 'src/models/IAddressFinder';
import { IStakeholder, IStakeholderSearch } from 'src/models/IStakeholder';
import { SelectionRefinement } from '@appology/react-components';
import { addressApi } from "src/Api/AddressApi";
import { Stakeholders } from "src/Enums/Stakeholders";
import { googleApi, IGoogleAddressGeoResponse, IGoogleGeoLocation } from "src/Api/GoogleApi";

export interface IOwnProps {
    onRegistrationChange: (stakeholder: IStakeholderSearch) => void,
    stakeholderId: Stakeholders
}

export interface IOwnState {
    stakeholder: IStakeholder,
    addresses: IAddressSearch[],
    loadingAddresses: boolean,
    apiAddresses: boolean,
    filter: string
}

export default class RegistrationForm extends React.Component<IOwnProps, IOwnState> {
    constructor(props: IOwnProps) {
        super(props);
        this.state = {
            stakeholder: {
                id: "",
                stakeholderId: props.stakeholderId,
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
                country: "United Kingdom",
                apiLat: undefined,
                apiLng: undefined,
                apiFormattedAddress: ""
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
            <div className="toggleswitch-margin-top">
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

        googleApi.getAddressGeo(`${address[0].addressline1} ${address[0].postcode}`)
            .then(data => this.googleGeometrySuccess(data))

        if (address.length === 1) {
            this.setState(prevState => ({
                stakeholder: { ...prevState.stakeholder,    
                    address1: address[0].addressline1,
                    town: address[0].posttown,
                    county: address[0].county,
                    postcode: address[0].postcode,
                    apiFormattedAddress: address[0].summaryline
                }
            }))
        }

        this.renderAddressDetails();
    }

    private googleGeometrySuccess = (data: IGoogleAddressGeoResponse) => {
        if (data.results.length === 1) {
            const geometry : IGoogleGeoLocation = { 
                lat: data.results[0].geometry.location.lat,
                lng: data.results[0].geometry.location.lng
            }

            this.setState({ 
                stakeholder: { ...this.state.stakeholder,
                    apiLat: Number(geometry.lat),
                    apiLng: Number(geometry.lng)
                }
            })
        }
    }

    private handleChange(event: React.ChangeEvent<HTMLInputElement>): void {
        this.setState({
            stakeholder: { ...this.state.stakeholder,    
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
        if (this.state.stakeholder.address1 || !this.state.apiAddresses) {
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
                    <input value={this.state.stakeholder[input]} onChange={this.handleChange} type="text" name={input} className="form input100" required={input === "address1" ? true : false} disabled={(input === "country")? true : false} />
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
        if (this.state.stakeholder !== null) {
            if (this.state.stakeholder.address1 === "") {
                alert ("Address must be selected");
                return false;
            }
            else {
                api.registerStakeholder(this.state.stakeholder as IStakeholder)
                    .then(c => this.stakeholderRegistrationSuccess(c));
            }
        }
        return false;
    }

    private stakeholderRegistrationSuccess = (c: IStakeholderRegisterResponse) => {
        if (c.stakeholder != null) {
            const stakeholderSearch: IStakeholderSearch = {
                stakeholder: c.stakeholder,
                id: c.stakeholder.id,
                name: stakeholderSearchTxt(c.stakeholder)
            }

            this.props.onRegistrationChange(stakeholderSearch);
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