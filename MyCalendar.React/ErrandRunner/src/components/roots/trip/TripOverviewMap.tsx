import * as React from 'react';
import { googleApi } from 'src/Api/GoogleApi';

interface IOwnProps {
    originPlaceId: string,
    destinationLat: number,
    destinationLng: number,
    width?: number,
    height?: number
}

export const TripOverviewMap: React.FC<IOwnProps> = (props) => {
    return (
        <iframe
            width={props.width ? props.width : 250} height={props.height ? props.height : "250"} frameBorder="0" style={{"border": "0"}}
            src={`https://www.google.com/maps/embed/v1/directions?key=${googleApi.googleMapsApiKey}&origin=place_id:${props.originPlaceId}&destination=${props.destinationLat},${props.destinationLng}`} allowFullScreen={true} />
    )
}