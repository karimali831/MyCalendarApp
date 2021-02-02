import * as React from "react";
import { withScriptjs, withGoogleMap, GoogleMap, Circle, DirectionsRenderer } from "react-google-maps";
import { googleApi, IGoogleGeoLocation } from "src/Api/GoogleApi";
import { MarkerWithLabel } from 'react-google-maps/lib/components/addons/MarkerWithLabel';
import { IGoogleMapMarker } from "src/models/IGoogleMapMarker";
import { rootUrl } from "../utils/Utils";
import { IStakeholder } from "src/models/IStakeholder";
import { Load, ToggleSwitch } from "@appology/react-components";


export interface IOwnProps {
    marks: IGoogleMapMarker[],
    excludeCenterMarker: boolean,
    centerRadius: number,
    pickupLocation?: IGoogleGeoLocation,
    selectedCustomer?: IStakeholder,
    selectedDriver?: IStakeholder,
    selectedDriverAssigned: (stakeholder: IStakeholder | undefined) => void,
}

export interface IOwnState {
    directions?: google.maps.DirectionsResult,
    showDirections: boolean
}

export default class GoogleMapMarker extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);
        this.state = {
            directions: undefined,
            showDirections: false
        };
    }

    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (prevProps.marks.length > 0 && prevProps.marks !== this.props.marks) {
            this.setDirections();
        }
    
        if (this.props.pickupLocation !== undefined && JSON.stringify(this.props.pickupLocation) !== JSON.stringify(prevProps.pickupLocation)) {
            this.resetDirections();
        }

        if (this.props.selectedCustomer !== undefined && this.props.selectedCustomer?.id !== prevProps.selectedCustomer?.id) {
            this.resetDirections();
        }
    }


    public render() {
        const Map = withScriptjs(
            withGoogleMap((props: any) => (
                <GoogleMap
                    defaultZoom={12}
                    defaultCenter={this.props.marks.find(m => m.centerMark !== undefined)?.centerMark} >
                    {
                        this.state.directions !== undefined && this.state.showDirections ?
                            <DirectionsRenderer directions={this.state.directions} />
                        : 
                        this.props.marks.map((mark: IGoogleMapMarker, index: number) => {
                            let driverPosition: IGoogleGeoLocation | undefined;
                            let customerPosition: IGoogleGeoLocation | undefined; 

                            if (mark.driver !== undefined && mark.driver?.apiLat !== undefined && mark.driver.apiLng !== undefined) {
                                driverPosition = { 
                                    lat: mark.driver.apiLat, 
                                    lng: mark.driver.apiLng
                                }
                            }

                            if (mark.customer !== undefined && mark.customer?.apiLat !== undefined && mark.customer.apiLng !== undefined) {
                                customerPosition = { 
                                    lat: mark.customer.apiLat, 
                                    lng: mark.customer.apiLng
                                }
                            }

                            return (
                                <>
                                    {
                                        mark.centerMark !== undefined ?
                                            <Circle
                                                key={index}
                                                center={mark.centerMark}
                                                radius={this.props.centerRadius}
                                                options={{
                                                    strokeColor: "#09c",
                                                    strokeOpacity: 0.8,
                                                    strokeWeight: 2,
                                                    fillColor: `#09c`,
                                                    fillOpacity: 0.35,
                                                    zIndex: 1
                                                }}
                                            /> 
                                        : null 
                                    }
                                    {
                                        (this.props.excludeCenterMarker === false && mark.centerMark !== undefined) || customerPosition !== undefined ?
                                            <MarkerWithLabel
                                                labelStyle={{ borderRadius: '5px', textAlign: "center", width: '100px', backgroundColor: mark.centerMark !== undefined ? "#27ae60" : "#09c", color: "#fff", fontSize: "10px", padding: "5px"}}
                                                labelAnchor={{ x: (100/2) + 5 , y: mark.centerMark !== undefined ? 70 : 55 }}
                                                key={index}
                                                icon={ mark.centerMark !== undefined ? null : { url: `${rootUrl}/content/img/customer.png` } }
                                                position={mark.centerMark !== undefined ? mark.centerMark : customerPosition} 
                                            >
                                                <span>{mark.label}</span>
                                            </MarkerWithLabel>

                                        : driverPosition !== undefined ?

                                            <MarkerWithLabel
                                                labelStyle={{ borderRadius: '5px', fontWeight: mark.selected ? "bold" : "normal", textAlign: "center", width: '100px', backgroundColor: mark.selected ? "#ff0000" : "#000", color: "#fff", fontSize: "10px", padding: "5px"}}
                                                labelAnchor={{ x: (100/2) + 5 , y: 60 }}
                                                key={index}
                                                onClick={(e: React.ChangeEvent<MouseEvent>) => this.props.selectedDriverAssigned(mark.driver)}
                                                icon={{ url: `${rootUrl}/content/img/${mark.selected ? "runner-selected.png" : "runner.png"}`}}
                                                position={driverPosition} 
                                            >
                                                <span>{mark.label} {mark.selected ? "Assigned" : "Driver"}</span>
                                            </MarkerWithLabel>
                                        : null
                                    }
                                </>
                            )
                        
                        }
                    )}
                </GoogleMap>
            ))
        )
        return (
            <>
                {
                    this.props.selectedDriver !== undefined ?
                        <ToggleSwitch
                            id="directions"
                            name="Show directions"
                            inline={true}
                            checked={this.state.showDirections}
                            onChange={this.showDirections}
                        />
                    : null
                }
                {
                    this.props.marks.length > 0 ?
                        <Map
                            googleMapURL={`https://maps.googleapis.com/maps/api/js?key=${googleApi.googleMapsApiKey}`}
                            loadingElement={<Load withBackground={true} />}
                            containerElement={<div style={{ height: `550px` }} />}
                            mapElement={<div style={{ height: `100%` }} />}
                        />
                    : null
                }
            </>
        );
    }

    private resetDirections = () => {
        this.setState({ 
            directions: undefined,
            showDirections: false
         })
    }

    private showDirections = (checked: boolean) => {
        this.setState({ showDirections: checked })

        if (checked) {
            this.setDirections();
        }
    }

    private setDirections = () => {
        const selectedDriver = this.props.selectedDriver;
        const selectedCustomer = this.props.selectedCustomer;

        if (selectedDriver !== undefined && selectedDriver.apiLat !== undefined && selectedDriver.apiLng !== undefined) {
            if (selectedCustomer !== undefined && selectedCustomer.apiLat !== undefined && selectedCustomer.apiLng !== undefined) {

                const directionsService = new window.google.maps.DirectionsService();

                const origin = { lat: selectedDriver.apiLat, lng: selectedDriver.apiLng };
                const destination = { lat: selectedCustomer.apiLat, lng: selectedCustomer.apiLng }
            
                if (this.props.pickupLocation !== undefined) {

                    directionsService.route(
                        {
                            origin: origin,
                            destination: destination,
                            travelMode: google.maps.TravelMode.DRIVING,
                            waypoints: [
                                {
                                    location: new google.maps.LatLng(this.props.pickupLocation.lat,  this.props.pickupLocation.lng)
                                }
                            ]
                        },
                        (result, status) => {

                            if (status === google.maps.DirectionsStatus.OK) {
                                this.setState({ directions: result});
                            } else {
                                console.error(`error fetching directions ${result}`);
                            }
                        }
                    );
                }
            }
        }
    }
}