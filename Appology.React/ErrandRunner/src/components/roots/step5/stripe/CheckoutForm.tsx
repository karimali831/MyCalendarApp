import { Variant } from '@appology/react-components';
import { CardElement } from '@stripe/react-stripe-js';
import { StripeElements, Stripe, StripeElementChangeEvent } from '@stripe/stripe-js';
import * as React from 'react';
import { IPaymentIntentRequest, IPaymentIntentResponse, stripeApi } from 'src/Api/StripeApi';
import { showAlert } from 'src/components/utils/Utils';

export interface IOwnProps {
    elements: StripeElements | null;
    stripe: Stripe | null;
    orderId: string,
    invoice: number
}

export interface IOwnState {
    clientSecret: string,
    disabled: boolean,
    error?: string,
    succeeded: boolean,
    processing: boolean
}


export class CheckoutForm extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            clientSecret: "",
            disabled: true,
            error: undefined,
            succeeded: false,
            processing: false
        };
    }

    public componentDidMount() {
        this.createPaymentIntent();
    }

    public render() {
        const { processing, disabled, succeeded, error } = this.state;

        const cardStyle = {
            style: {
              base: {
                color: "#32325d",
                fontFamily: 'Arial, sans-serif',
                fontSmoothing: "antialiased",
                fontSize: "16px",
                "::placeholder": {
                  color: "#32325d"
                }
              },
              invalid: {
                color: "#fa755a",
                iconColor: "#fa755a"
              }
            }
          };
        
        return (
            <form id="payment-form" onSubmit={this.handleSubmit}>
                <CardElement id="card-element" options={cardStyle} onChange={this.handleChange} />
                <button disabled={processing || disabled || succeeded || error !== undefined} id="submit">
                <span id="button-text">
                    {processing ? <div className="spinner" id="spinner" />  : "Pay" }
                </span>
                </button>
                { error && showAlert(error, Variant.Danger) }
                { succeeded && showAlert("Payment successful") }
            </form>
        );
    }

    private handleChange = async (event: StripeElementChangeEvent) => {
        this.setState({ 
            disabled: event.empty,
            error: event.error ? event.error.message : ""
        })
    }

    private createPaymentIntent = () => {
        const paymentIntentRequest : IPaymentIntentRequest = {
            orderId: this.props.orderId,
            invoice: this.props.invoice
        }

        stripeApi.paymentIntent(paymentIntentRequest)
            .then(r => this.createPaymentIntentSuccess(r))
    }    

    private createPaymentIntentSuccess = (response: IPaymentIntentResponse) => {
        this.setState({ 
            clientSecret: response.clientSecret,
            error: response.status ? undefined : "Error creating payment intent"
         });
    }

    private handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        this.setState({ processing: true })

        const { stripe, elements } = this.props;
    
        if (!stripe || !elements) {
          return;
        }

        const cardElement = elements.getElement(CardElement);

        if (!cardElement) {
            return;
        }

        const payload = await stripe.confirmCardPayment(this.state.clientSecret, {
            payment_method: {
                card: cardElement
            }
        });

        if (payload.error) {
            this.setState({
                error: `Payment failed ${payload.error.message}`,
                processing: false
            })
        } 
        else{
            this.setState({
                error: undefined,
                processing: false,
                succeeded: true
            })
        }
    };
}

