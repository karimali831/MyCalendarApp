import * as React from 'react';
import Card from 'react-bootstrap/Card'
import CardGroup from 'react-bootstrap/CardGroup'
import { FaCar, FaFileInvoice, FaPoundSign, FaSlidersH } from 'react-icons/fa';
import { IOrderForm } from 'src/models/IOrder';
import { IOrderOverview } from 'src/models/IOrder';
import { ITripOverview } from 'src/models/ITrip';
import { TripOverviewDetails } from '../trip/TripOverviewDetails';
// import { TripOverviewMap } from '../trip/TripOverviewMap';

interface IOwnProps {
    trip: ITripOverview,
    order: IOrderForm,
    mov: number,
    orderOverview: IOrderOverview,
    toggleConfig: () => void
}

export const OrderOverview: React.FC<IOwnProps> = (props) => {
    return (
        <CardGroup>
            {/* <Card>
                <Card.Header>
                    <FaMap /> <b>Map Directions</b>
                </Card.Header>
                <Card.Body>
                    <Card.Text>
                        <TripOverviewMap
                            originPlaceId={props.trip.pickupId}
                            destinationLat={props.trip.stakeholderLocation.lat}
                            destinationLng={props.trip.stakeholderLocation.lng}
                            width={300}
                            height={200} />
                    </Card.Text>
                </Card.Body>
            </Card> */}
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
                    <FaSlidersH className="float-right" onClick={props.toggleConfig} />
                </Card.Header>
                <Card.Body>
                    <Card.Text>
                        Order Value <span style={{float: "right", fontWeight: "bold"}}>£{props.order.orderValue.toFixed(2)}</span> <br />
                        Service Fee <span style={{float: "right", fontWeight: "bold"}}>£{props.orderOverview.serviceFee.toFixed(2)}</span> <br />
                        Order Fee <span style={{float: "right", fontWeight: "bold"}}>£{props.order.orderFee.toFixed(2)}</span> <br />
                        Delivery Fee <span style={{float: "right", fontWeight: "bold"}}>£{props.orderOverview.deliveryFee.toFixed(2)}</span> <br />
                        Remaining MOV  <span style={{float: "right", fontWeight: "bold"}}>£{(props.mov < 0 ? 0 : props.mov).toFixed(2)}</span>
                    </Card.Text>
                </Card.Body>
            </Card>
            <Card>
                <Card.Header>
                    <FaFileInvoice /> <b>Invoice</b>
                </Card.Header>
                <Card.Body>
                    <Card.Text>
                        Total Items <span style={{float: "right", fontWeight: "bold"}}>{props.order.totalItems}</span> <br />
                        Invoice <span style={{float: "right", fontWeight: "bold"}}>£{props.orderOverview.invoiceAmt.toFixed(2)}</span><br />
                        {/* VAT <span style={{float: "right", fontWeight: "bold"}}>£{(props.invoiceAmt * 0.20).toFixed(2)}</span><br /> */}
                        NET Profit <span style={{float: "right", fontWeight: "bold"}}>£{props.orderOverview.netProfit.toFixed(2)}</span> <br />
                        Driver fee <span style={{float: "right", fontWeight: "bold"}}>£{props.orderOverview.driverFee.toFixed(2)}</span> <br />
                        Driver earning <span style={{float: "right", fontWeight: "bold"}}>£{(props.orderOverview.netProfit - props.orderOverview.driverFee).toFixed(2)}</span>
                    </Card.Text>
                </Card.Body>
            </Card>
        </CardGroup>
    )
}