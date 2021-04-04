import { Load, SelectElement, SelectionRefinement, ToggleSwitch } from "@appology/react-components";
import IBaseModel from "@appology/react-components/dist/SelectionRefinement/IBaseModel";
import * as React from "react";
import Card from "react-bootstrap/esm/Card";
import CardGroup from "react-bootstrap/esm/CardGroup";
import { FaCheckCircle, FaInfoCircle, FaStore } from "react-icons/fa";
import { api } from "src/Api/Api";
import { googleApi, IGoogleGeoLocation } from "src/Api/GoogleApi";
import { rootUrl } from "src/components/utils/Utils";
import { IGoogleAutoCompleteSearch, IPrediction } from 'src/models/IGoogleAutoComplete';
import { IPlace } from "src/models/IPlace";
import { IStakeholder } from "src/models/IStakeholder";
import { DistanceMatrixAction, LoadPlacesAction, PlaceAction, SearchStoreAction, SelectedServiceAction, SetActiveStepAction } from "src/state/contexts/landing/Actions";
import ServiceConnected from "../service/ServiceConnected";
import Button from 'react-bootstrap/Button'

export interface IPropsFromDispatch {
    onStoreChange: (store: IGoogleAutoCompleteSearch, storeLocation: IGoogleGeoLocation, stakeholderLocation: IGoogleGeoLocation) => DistanceMatrixAction
    onPlaceChange: (place: IPlace | undefined) => PlaceAction,
    selectedServiceChange: (service: IBaseModel | undefined) => SelectedServiceAction,
    searchStoreChange: (searchStore: boolean) => SearchStoreAction,
    loadPlaces: (places: IPlace[]) => LoadPlacesAction,
    goToStep: (step: number) => SetActiveStepAction
}

export interface IPropsFromState {
    customer?: IStakeholder,
    selectedService?: IBaseModel,
    searchStore: boolean,
    selectedStore?: IGoogleAutoCompleteSearch,
    places: IPlace[]
}

export interface IOwnState {
    filter: string,
    stores: IGoogleAutoCompleteSearch[],
    loadingStores: boolean,
    loadingPlaces: boolean,
    loadingPlace: boolean,
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
            loadingStores: false,
            loadingPlaces: false,
            loadingPlace: false,
            stakeholderLocation: undefined,
            radius: {
                id: "8046",
                name: "5 miles"
            }
        }
    }

    public componentDidMount() {
        if (this.props.places.length === 0) {
            this.getPlaces();
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
                <ToggleSwitch
                    id="searchstore"
                    name="Manually search store"
                    inline={true}
                    checked={this.props.searchStore}
                    onChange={(v: boolean) => this.props.searchStoreChange(v)} 
                />
                <hr />
                {
                    !this.props.searchStore ?
                        <CardGroup>
                            {
                                this.state.loadingPlaces ? 
                                    <Card style={{ maxWidth: 250 }}>
                                        <Card.Body>
                                        <Card.Title>Loading stores...</Card.Title>
                                            <Card.Text>
                                                <Load />
                                            </Card.Text>
                                        </Card.Body>
                                    </Card> :

                                    this.props.places.length > 0 && this.props.places.map(p => {
                                        return (
                                            <Card key={p.id} style={{ maxWidth: 250 }}>
                                                <Card.Img variant="top" src={`${rootUrl}/${p.imagePath}`} style={{ width: "100%", height: 100 }} />
                                                <Card.Body>
                                                <Card.Title>{p.name}</Card.Title>
                                                    <Card.Text>
                                                        {p.description} 
                                                        <hr />
                                                        <span className="selected-store">
                                                        {
                                                            p.placeId === this.props.selectedStore?.id ? 
                                                                <Button onClick={() => this.props.goToStep(2)} variant="success">
                                                                    <FaCheckCircle /> Store Selected
                                                                </Button> 
                                                            : p.active ?
                                                                <Button onClick={() => this.selectedPlace(p)}>
                                                                    {
                                                                        this.state.loadingPlace ? 
                                                                            <Load smallSize={true} inlineDisplay={true} /> :
                                                                            <><FaStore /> Select Store</>
                                                                    }
                                                                </Button> 
                                                            :
                                                                <Button variant="info">
                                                                    <FaInfoCircle /> We'll be back!
                                                                </Button>
                                                        }
                                                    </span>
                                                    </Card.Text>
                                                </Card.Body>
                                            </Card>
                                        )
                                    })
                            }
                        </CardGroup>
                    :
                    <>
                        <ServiceConnected />
                        {
                            this.props.selectedService ?
                            <>
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
                                    resultsLeftIcon={<FaStore />}
                                    filter={this.props.selectedStore?.name ?? this.state.filter} 
                                    onChange={(f) => this.pickupPlaceChanged(f)} 
                                    setFilterToItemName={true}
                                    itemSelected={(i) => this.selectedStore(i)}
                                    loading={this.state.loadingStores}
                                    filteredResults={this.state.stores} />
                            </>
                            : null
                        }
                    </>
                }
            </div>  
        );
    }

    private getPlaces() {
        this.setState({ loadingPlaces: true })

        api.places()
            .then(places => this.placesSuccess(places))
    }

    private placesSuccess = (places: IPlace[]) => {
        this.props.loadPlaces(places);
        this.setState({ loadingPlaces: false})
    }

    private selectedPlace = (place: IPlace) => {
        this.setState({ 
            loadingPlace: true,
            filter: place.description 
        })

        const store : IGoogleAutoCompleteSearch = {
            id: place.placeId,
            name: place.description
        }

        const service : IBaseModel = {
            id: place.serviceId.toString(),
            name: place.serviceName
        }

        this.props.selectedServiceChange(service)
        this.placeDetails(store)
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
            loadingStores: true,
            filter: ""
        })

        this.placeDetails(store)
    }

    private placeDetails = (store: IGoogleAutoCompleteSearch) => {

        // this is to see if we have any API data in ER.Places
        const place : IPlace | undefined = this.props.places.find(x => x.placeId === store.id)
        this.props.onPlaceChange(place);

        googleApi.placeDetails(store.id)
            .then(p => this.selectedStoreSuccess(store, p.result.geometry.location))
    }

    private selectedStoreSuccess = (store: IGoogleAutoCompleteSearch, storeLocation: IGoogleGeoLocation) => {
        if (this.state.stakeholderLocation !== undefined) {
            this.props.onStoreChange(store, storeLocation, this.state.stakeholderLocation);
            this.setState({ loadingPlace: false })
        }
    }

    private pickupPlaceChanged = (filter: string) => {
        this.setState({ 
            loadingStores: true,
            filter: filter 
        })
    }

    private autoCompleteSuccess = (stores: IPrediction[]) => {
        this.setState({ ...this.state,
            ...{ 
                loadingStores: false,
                stores: stores.map(store =>({ 
                    id: store.place_id,
                    name: store.description
                } as IGoogleAutoCompleteSearch))
            }
        }) 
    }
}