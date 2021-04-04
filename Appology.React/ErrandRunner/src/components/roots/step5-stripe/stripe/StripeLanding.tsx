import { Variant } from '@appology/react-components';
import { Elements, ElementsConsumer } from '@stripe/react-stripe-js';
import { loadStripe} from '@stripe/stripe-js';
import * as React from 'react';
import { OrderAction } from 'src/models/IDispatchStatus';
import { ToggleAlertAction } from 'src/state/contexts/landing/Actions';
import { OrderPaidAction, OrderStatusAction } from 'src/state/contexts/order/Actions';
import { CheckoutForm } from './CheckoutForm';
import './Style.css';


export interface IOwnProps {
    orderId: string,
    invoice: number,
    orderStatusChange: (action: OrderAction, variant: Variant, show: boolean) => OrderStatusAction,
    handleAlert: (text: string, variant?: Variant, timeout?: number) => ToggleAlertAction,
    orderPaid: (paid: boolean, stripePaymentConfirmationId?: string) => OrderPaidAction
}

// Make sure to call `loadStripe` outside of a component’s render to avoid
// recreating the `Stripe` object on every render.
const stripePromise = loadStripe('pk_test_51HlxO9BR6iw4dg3GdRCE4AZuvoRP5a6XMNwKdaJ0quIpzlJ0uFDAd0DhfWyEAUyVna5mKkwaNPYHRjcxqTpu9klU00uTGoVH1n');

export const StripePayment: React.FC<IOwnProps> = (props: IOwnProps) => {
    return (
        <Elements stripe={stripePromise}>
            <InjectedCheckoutForm 
                orderPaid={props.orderPaid}
                orderStatusChange={props.orderStatusChange}
                handleAlert={props.handleAlert} 
                orderId={props.orderId} 
                invoice={props.invoice} />
        </Elements>
    )
}

const InjectedCheckoutForm = (props: IOwnProps) => {
    return (
        <ElementsConsumer>
            {
                ({elements, stripe}) => (
                    <CheckoutForm 
                        orderPaid={props.orderPaid}
                        orderStatusChange={props.orderStatusChange}
                        handleAlert={props.handleAlert}
                        elements={elements} 
                        stripe={stripe} 
                        orderId={props.orderId}
                        invoice={props.invoice}
                    />
                )
            }
        </ElementsConsumer>
    );
};