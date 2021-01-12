import * as React from 'react';
import Card from 'react-bootstrap/Card'
import CardGroup from 'react-bootstrap/CardGroup'
import { FaCar, FaFileInvoice, FaMap, FaPoundSign } from 'react-icons/fa';
import { ITripOverview } from 'src/models/TripOverview';
import { TripOverviewDetails } from '../trip/TripOverviewDetails';
import { TripOverviewMap } from '../trip/TripOverviewMap';

interface IOwnProps {
    trip: ITripOverview,
    itemsQuantity: number,
    totalItemsCost: number,
    serviceFee: number,
    orderFee: number,
    deliveryFee: number,
    tripMileage: number,
    invoiceAmt: number,
    netProfit: number,
    driverFee: number,
}

export const OrderOverview: React.FC<IOwnProps> = (props) => {
    return (
        <CardGroup>
            <Card>
                <Card.Header>
                    <FaMap /> <b>Map Directions</b>
                </Card.Header>
                <Card.Body>
                    <Card.Text>
                        <TripOverviewMap
                            originPlaceId={props.trip.pickupId}
                            destinationLat={props.trip.customerLocation.lat}
                            destinationLng={props.trip.customerLocation.lng}
                            width={300}
                            height={200} />
                    </Card.Text>
                </Card.Body>
            </Card>
            <Card>
                <Card.Header>
                    <FaCar /> <b>Driver Trip</b>
                </Card.Header>
                <Card.Body>
                    <Card.Text>
                        <TripOverviewDetails trip={props.trip} />
                    </Card.Text>
                </Card.Body>
            </Card>
            <Card>
                <Card.Header>
                    <FaPoundSign /> <b>Service Costs</b>
                </Card.Header>
                <Card.Body>
                    <Card.Text>
                        Order Value <span style={{float: "right", fontWeight: "bold"}}>£{props.totalItemsCost.toFixed(2)}</span> <br />
                        Service Fee <span style={{float: "right", fontWeight: "bold"}}>£{props.serviceFee.toFixed(2)}</span> <br />
                        Order Fee <span style={{float: "right", fontWeight: "bold"}}>£{props.orderFee.toFixed(2)}</span> <br />
                        Delivery Fee <span style={{float: "right", fontWeight: "bold"}}>£{props.deliveryFee.toFixed(2)}</span>
                    </Card.Text>
                </Card.Body>
            </Card>
            <Card>
                <Card.Header>
                    <FaFileInvoice /> <b>Invoice</b>
                </Card.Header>
                <Card.Body>
                    <Card.Text>
                        Total Items <span style={{float: "right", fontWeight: "bold"}}>{props.itemsQuantity}</span> <br />
                        Invoice <span style={{float: "right", fontWeight: "bold"}}>£{props.invoiceAmt.toFixed(2)}</span><br />
                        {/* VAT <span style={{float: "right", fontWeight: "bold"}}>£{(props.invoiceAmt * 0.20).toFixed(2)}</span><br /> */}
                        NET Profit <span style={{float: "right", fontWeight: "bold"}}>£{props.netProfit.toFixed(2)}</span> <br />
                        Driver fee <span style={{float: "right", fontWeight: "bold"}}>£{props.driverFee.toFixed(2)}</span> <br />
                        Driver earning <span style={{float: "right", fontWeight: "bold"}}>£{(props.netProfit - props.driverFee).toFixed(2)}</span>
                    </Card.Text>
                </Card.Body>
            </Card>
        </CardGroup>
    )
}