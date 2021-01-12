import { SelectElement, SelectionRefinement } from "@appology/react-components";
import IBaseModel from "@appology/react-components/dist/SelectionRefinement/IBaseModel";
import * as React from "react";
import { googleApi, IGoogleAddressGeoResponse, IGoogleGeoLocation } from "src/Api/GoogleApi";
import { IGoogleAutoCompleteSearch, IPrediction } from 'src/models/IGoogleAutoComplete';


export interface IOwnProps {
    addressLine1: string,
    postCode: string,
    onStoreChange: (store: IGoogleAutoCompleteSearch, customerLocation?: IGoogleGeoLocation) => void
}

export interface IOwnState {
    filter: string,
    stores: IGoogleAutoCompleteSearch[]
    loading: boolean,
    customerLocation?: IGoogleGeoLocation,
    radius: IBaseModel
}

export default class PickupLocation extends React.Component<IOwnProps, IOwnState> {

    constructor(props: any) {
        super(props);
        this.state = {
            filter: "",
            stores: [],
            loading: false,
            customerLocation: undefined,
            radius: {
                id: "8046",
                name: "5 miles"
            }
        }
    }

    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (prevState.filter !== this.state.filter && this.state.filter !== "") {
            this.googleGeometry();
        }
    }

    public render(): JSX.Element {

        const radiusOptions : IBaseModel[] = [
            {id: "4828", name: "3 miles"}, 
            {id: "6437", name: "4 miles"},
            {id: "8046", name:"5 miles"}
        ] 
  
        return (
            <div>
                <SelectElement 
                    label="Stores proximity from customer address"
                    icon="&#xf299;"
                    id="radius"
                    selected={this.state.radius.id}
                    selectorOptions={radiusOptions}
                    onSelectChange={this.handleRadiusChange}
                />

                <SelectionRefinement<IGoogleAutoCompleteSearch>
                    label="Store Selection"
                    placeholder="Search establishment..."
                    filter={this.state.filter} 
                    onChange={(f) => this.pickupLocationChanged(f)} 
                    setFilterToItemName={true}
                    itemSelected={(i) => this.selectedStore(i)}
                    loading={this.state.loading}
                    filteredResults={this.state.stores} />
            </div>
                    
        );
    }

    private handleRadiusChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
        const radius : IBaseModel = {
            id: event.target.value,
            name: event.target.name
        }
        this.setState({ radius: radius })
    }

    private googleGeometry = () => {
        googleApi.getAddressGeo(`${this.props.addressLine1} ${this.props.postCode}`)
            .then(data => this.googleGeometrySuccess(data))
    }

    private googleGeometrySuccess = (data: IGoogleAddressGeoResponse) => {
        if (data.results.length === 1) {
            const geometry : IGoogleGeoLocation = { 
                lat: data.results[0].geometry.location.lat,
                lng: data.results[0].geometry.location.lng
            }

            this.setState({ customerLocation: geometry })
            
            googleApi.googleAutoComplete(this.state.filter, geometry.lat, geometry.lng, this.state.radius.id)
                .then((c) => this.autoCompleteSuccess(c.predictions));
        }
    }


    private selectedStore = (store: IGoogleAutoCompleteSearch) => {
        this.props.onStoreChange(store, this.state.customerLocation);

        this.setState({ 
            loading: true,
            filter: ""
        })
    }

    private pickupLocationChanged = (filter: string) => {
        this.setState({ 
            loading: true,
            filter: filter 
        })
    }

    private autoCompleteSuccess = (stores: IPrediction[]) => {
        this.setState({ ...this.state,
            ...{ 
                loading: false,
                stores: stores.map(store =>({ 
                    id: store.place_id,
                    name: store.description
                } as IGoogleAutoCompleteSearch))
            }
        }) 
      }

}