import * as React from 'react';
import { FaInfo } from 'react-icons/fa';
import OverlayTrigger from 'react-bootstrap/OverlayTrigger'
import Popover from 'react-bootstrap/Popover'
import { TripOverviewDetails } from './TripOverviewDetails';
import { ITripOverview } from 'src/models/TripOverview';

interface IOwnProps {
    trip: ITripOverview
}

export const TripOverview: React.FC<IOwnProps> = (props) => {
    return (
        <OverlayTrigger
            trigger="click"
            key="top"
            placement="top"
            overlay={
                <Popover id={`popover-positioned-top`}>
                    <Popover.Title as="h3">Trip Overview</Popover.Title>
                    <Popover.Content>
                        <TripOverviewDetails trip={props.trip} />
                    </Popover.Content>
                </Popover>
            }>
            <span className="badge badge-primary" style={{ cursor: "pointer"}}>
                <FaInfo /> Pick up from {props.trip.pickupLocation}
            </span>
        </OverlayTrigger>
    )
}