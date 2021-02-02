import { SelectElement, SelectionRefinement } from "@appology/react-components";
import IBaseModel from "@appology/react-components/dist/SelectionRefinement/IBaseModel";
import * as React from "react";
import { googleApi, IGoogleGeoLocation } from "src/Api/GoogleApi";
import { IGoogleAutoCompleteSearch, IPrediction } from 'src/models/IGoogleAutoComplete';
import { IStakeholder } from "src/models/IStakeholder";


export interface IPropsFromDispatch {
    onStoreChange: (store: IGoogleAutoCompleteSearch, storeLocation: IGoogleGeoLocation, stakeholderLocation: IGoogleGeoLocation) => void
}

export interface IPropsFromState {
    customer?: IStakeholder
}

export interface IOwnState {
    filter: string,
    stores: IGoogleAutoCompleteSearch[]
    loading: boolean,
    stakeholderLocation?: IGoogleGeoLocation,
    radius: IBaseModel
}

type AllProps = IPropsFromState & IPropsFromDispatch;

export default class PickupLocation extends React.Component<AllProps, IOwnState> {

    constructor(props: any) {
        super(props);
        this.state = {
            filter: "",
            stores: [],
            loading: false,
            stakeholderLocation: undefined,
            radius: {
                id: "8046",
                name: "5 miles"
            }
        }
    }

    public componentDidUpdate = (prevProps: AllProps, prevState: IOwnState) => {
        if (prevState.filter !== this.state.filter && this.state.filter !== "") {
            if (this.props.customer?.apiLat !== undefined && this.props.customer?.apiLng !== undefined) {
                this.googleGeometry(this.props.customer.apiLat, this.props.customer.apiLng)
            }
            else{
                alert("CUSTOMER DETAILS DOES NOT HAVE APILAT API LNG - GET GEO DATA FROM API");
                googleApi.getAddressGeo(`${this.props.customer?.address1} ${this.props.customer?.postcode}`)
                    .then(data => this.googleGeometry(data.results[0].geometry.location.lat, data.results[0].geometry.location.lng))
            }
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
                    label="Stores proximity from customer location"
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
                    onChange={(f) => this.pickupPlaceChanged(f)} 
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

    private googleGeometry = (lat: number, lng: number) => {
            const geometry : IGoogleGeoLocation = { lat: lat, lng: lng }
            this.setState({ stakeholderLocation: geometry })

            googleApi.googleAutoComplete(this.state.filter, geometry.lat, geometry.lng, this.state.radius.id)
                .then((c) => this.autoCompleteSuccess(c.predictions));
        
    }

    private selectedStore = (store: IGoogleAutoCompleteSearch) => {
        this.setState({ 
            loading: true,
            filter: ""
        })

        googleApi.placeDetails(store.id)
            .then(p => this.selectedStoreSuccess(store, p.result.geometry.location))
    }

    private selectedStoreSuccess = (store: IGoogleAutoCompleteSearch, storeLocation: IGoogleGeoLocation) => {
        if (this.state.stakeholderLocation !== undefined) {
            this.props.onStoreChange(store, storeLocation, this.state.stakeholderLocation);
        }
    }

    private pickupPlaceChanged = (filter: string) => {
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