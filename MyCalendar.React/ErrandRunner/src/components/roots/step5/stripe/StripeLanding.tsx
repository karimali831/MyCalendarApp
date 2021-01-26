import { Elements, ElementsConsumer } from '@stripe/react-stripe-js';
import { loadStripe} from '@stripe/stripe-js';
import * as React from 'react';
import { CheckoutForm } from './CheckoutForm';
import './Style.css';


export interface IOwnProps {
    orderId: string,
    invoice: number
}

// Make sure to call `loadStripe` outside of a component’s render to avoid
// recreating the `Stripe` object on every render.
const stripePromise = loadStripe('pk_test_51HlxO9BR6iw4dg3GdRCE4AZuvoRP5a6XMNwKdaJ0quIpzlJ0uFDAd0DhfWyEAUyVna5mKkwaNPYHRjcxqTpu9klU00uTGoVH1n');

export const Payment: React.FC<IOwnProps> = (props: IOwnProps) => {
    return (
        <Elements stripe={stripePromise}>
            <InjectedCheckoutForm orderId={props.orderId} invoice={props.invoice} />
        </Elements>
    )
}

const InjectedCheckoutForm = (props: IOwnProps) => {
    return (
        <ElementsConsumer>
            {
                ({elements, stripe}) => (
                    <CheckoutForm 
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