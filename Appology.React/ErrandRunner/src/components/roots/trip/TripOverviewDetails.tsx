import * as React from 'react';
import { FaCar } from 'react-icons/fa';
import { ITripOverview } from 'src/models/ITrip';

interface IOwnProps {
    trip: ITripOverview
}

export const TripOverviewDetails: React.FC<IOwnProps> = (props) => {
    return (
        <div>
            <strong>Pickup From</strong><br /> {props.trip.pickupPlace} 
            <br /><br />
            <strong>Calculated Trip</strong><br /> {props.trip.distance} <FaCar /> {props.trip.duration}
            <br /><br />
            <strong>Customer Dropoff</strong><br /> {props.trip.dropOffAddress} {props.trip.dropOffPostCode}
        </div>
    )
}