import * as React from 'react';
import { StripePayment } from './stripe/StripeLanding';
import Card from 'react-bootstrap/Card'
import { IOrder, IOrderOverview } from 'src/models/IOrder';
import { ToggleAlertAction } from 'src/state/contexts/landing/Actions';
import { Load, Variant } from '@appology/react-components';
import CardDeck from 'react-bootstrap/esm/CardDeck';
import { DeliverySlots } from './delivery/DeliverySlots';
import { FaCheckCircle, FaCreditCard, FaStripe, FaTruck } from 'react-icons/fa';
import { OrderPaidAction, OrderStatusAction, SetDeliveryDateAction } from 'src/state/contexts/order/Actions';
import ChecklistConnected from './checklist/ChecklistConnected';
import { SaveStatus } from 'src/Enums/SaveStatus';
import { IDispatchStatus, OrderAction } from 'src/models/IDispatchStatus';
import Badge from 'react-bootstrap/Badge'

export interface IPropsFromDispatch {
    handleAlert: (text: string, variant?: Variant, timeout?: number) => ToggleAlertAction,
    setDeliveryDate: (deliveryDate?: Date, timeslot?: string) => SetDeliveryDateAction,
    orderStatusChange: (action: OrderAction, variant: Variant, show: boolean) => OrderStatusAction,
    orderPaid: (paid: boolean, stripePaymentConfirmationId?: string) => OrderPaidAction
}

export interface IPropsFromState {
    deliveryDate?: Date,
    timeslot?: string,
    order?: IOrder,
    orderOverview: IOrderOverview,
    saveOrderStatus: SaveStatus,
    orderStatus: IDispatchStatus[],
    stripeConfirmationPaymentId?: string
}

type AllProps = IPropsFromDispatch & IPropsFromState 

export const Overview: React.FC<AllProps> = (props) => {

    return (
        <CardDeck>
            <Card>
                <Card.Header>
                    <FaTruck /> <b>Delivery Slot</b>
                </Card.Header>
                <Card.Body>
                    <Card.Text>
                        <DeliverySlots 
                            order={props.order}
                            deliveryDate={props.deliveryDate}
                            selectedTimeslot={props.timeslot}
                            setDeliveryDate={props.setDeliveryDate}
                            orderStatusChange={props.orderStatusChange}
                            handleAlert={props.handleAlert} />
                    </Card.Text>
                </Card.Body>
            </Card>
            <Card>
                <Card.Header>
                    <FaStripe size={21} /> <b>Payment</b>
                </Card.Header>
                <Card.Body>
                    <Card.Text>
                        Delivery fee <span style={{float: "right", fontWeight: "bold"}}>£{props.orderOverview.deliveryFee.toFixed(2)}</span> <br />
                        Order total <span style={{float: "right", fontWeight: "bold"}}>£{props.orderOverview.invoiceAmt.toFixed(2)}</span><br /> 
                        <div className="float-right">
                        {
                            props.orderStatus.some(x => x.orderAction === OrderAction.Payment && x.variant === Variant.Success) ?
                            <>
                                {
                                    props.stripeConfirmationPaymentId ?
                                        <Badge variant="success"><FaStripe /> Paid with Stripe</Badge>
                                    :
                                        <Badge variant="danger" onClick={() => props.orderPaid(false)}>
                                            <FaCreditCard /> Mark as Unpaid
                                        </Badge>
                                } 
                            </>
                            :
                                <Badge variant="success" onClick={() => props.orderPaid(true)}>
                                    <FaCreditCard /> Mark as Paid
                                </Badge>
                        }
                        </div>
                        <br /><br />
                        {
                            props.order && !props.orderStatus.some(x => x.orderAction === OrderAction.Payment && x.variant === Variant.Success) &&
                            <StripePayment 
                                orderPaid={props.orderPaid}
                                orderStatusChange={props.orderStatusChange}
                                handleAlert={props.handleAlert} 
                                orderId={props.order?.orderId} 
                                invoice={props.orderOverview.invoiceAmt} 
                            />}
                    </Card.Text>
                </Card.Body>
            </Card>
            <Card>
                <Card.Header>
                    <FaCheckCircle /> <b>Status</b>
                    {
                        props.saveOrderStatus === SaveStatus.Processing && 
                        <span className="float-right"><Load smallSize={true} inlineDisplay={true} /></span>
                    }
                </Card.Header>
                <Card.Body>
                    <ChecklistConnected />
                </Card.Body>
            </Card>
        </CardDeck>
    )
}