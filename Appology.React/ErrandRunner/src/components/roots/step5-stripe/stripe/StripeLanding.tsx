import { Variant } from '@appology/react-components';
import { Elements, ElementsConsumer } from '@stripe/react-stripe-js';
import { loadStripe} from '@stripe/stripe-js';
import * as React from 'react';
import { ToggleAlertAction } from 'src/state/contexts/landing/Actions';
import { CheckoutForm } from './CheckoutForm';
import './Style.css';


export interface IOwnProps {
    orderId: string,
    invoice: number,
    handleAlert: (text: string, variant?: Variant, timeout?: number) => ToggleAlertAction
}

// Make sure to call `loadStripe` outside of a component’s render to avoid
// recreating the `Stripe` object on every render.
const stripePromise = loadStripe('pk_test_51HlxO9BR6iw4dg3GdRCE4AZuvoRP5a6XMNwKdaJ0quIpzlJ0uFDAd0DhfWyEAUyVna5mKkwaNPYHRjcxqTpu9klU00uTGoVH1n');

export const StripePayment: React.FC<IOwnProps> = (props: IOwnProps) => {
    return (
        <Elements stripe={stripePromise}>
            <InjectedCheckoutForm handleAlert={props.handleAlert} orderId={props.orderId} invoice={props.invoice} />
        </Elements>
    )
}

const InjectedCheckoutForm = (props: IOwnProps) => {
    return (
        <ElementsConsumer>
            {
                ({elements, stripe}) => (
                    <CheckoutForm 
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