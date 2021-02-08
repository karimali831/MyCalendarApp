import { PaymentRequestButtonElement } from '@stripe/react-stripe-js';
import { PaymentRequestOptions, Stripe, StripePaymentRequestButtonElementOptions } from '@stripe/stripe-js';
import * as React from 'react';

export interface IOwnState {
    canMakePayment: boolean,
    // paymentRequest: PaymentRequest
    paymentRequestOptions: StripePaymentRequestButtonElementOptions
}

export interface IOwnProps {
    stripe: Stripe
}


export class PaymentRequestForm extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        if (this.props.stripe !== null) {

            const paymentOptions : PaymentRequestOptions = {
                country: 'US',
                currency: 'usd',
                total: {
                    label: 'Demo total',
                    amount: 1000,
                }
            }

            const paymentRequest = this.props.stripe.paymentRequest(paymentOptions);

            paymentRequest.on('token', ({complete, token, ...data}) => {
                console.log('Received Stripe token: ', token);
                console.log('Received customer information: ', data);
                complete('success');
            });

            paymentRequest.canMakePayment().then((result: any) => {
                this.setState({
                    canMakePayment: !!result, 
                    paymentRequestOptions: {
                        paymentRequest: paymentRequest
                    }
                });
            });

        
        }
    }
    public render() {
        return this.state.canMakePayment ? (
            <PaymentRequestButtonElement
                options={this.state.paymentRequestOptions}
                className="PaymentRequestButton"
                // style={{
                //     // For more details on how to style the Payment Request Button, see:
                //     // https://stripe.com/docs/elements/payment-request-button#styling-the-element
                //     paymentRequestButton: {
                //     theme: 'light',
                //     height: '64px',
                //     },
                // }}
            />
        ) : null;
    }
}

export default PaymentRequestForm