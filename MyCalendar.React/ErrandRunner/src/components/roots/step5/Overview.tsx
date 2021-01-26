import * as React from 'react';
import { Payment } from './stripe/StripeLanding';
import Card from 'react-bootstrap/Card'
import { IOrderOverview } from 'src/models/IOrder';

export interface IPropsFromDispatch {

}

export interface IPropsFromState {
    orderId?: string,
    orderOverview: IOrderOverview
}

type AllProps = IPropsFromDispatch & IPropsFromState

export const Overview: React.FC<AllProps> = (props) => {

    if (!props.orderId) {
        alert("No valid order")
        return null;
    }

    return (
        <Card style={{ width: '24rem' }}>
            <Card.Header>
                <b>Order</b>
            </Card.Header>
            <Card.Body>
                <Card.Text>
                    Delivery fee <span style={{float: "right", fontWeight: "bold"}}>£{props.orderOverview.deliveryFee.toFixed(2)}</span> <br />
                    Order total <span style={{float: "right", fontWeight: "bold"}}>£{props.orderOverview.invoiceAmt.toFixed(2)}</span><br /> <br />
                    <Payment orderId={props.orderId} invoice={props.orderOverview.invoiceAmt} />
                </Card.Text>
            </Card.Body>
        </Card>
    )
}