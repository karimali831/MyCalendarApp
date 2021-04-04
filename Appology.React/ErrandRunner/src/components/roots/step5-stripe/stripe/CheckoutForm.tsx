import { Variant } from '@appology/react-components';
import { CardElement } from '@stripe/react-stripe-js';
import { StripeElements, Stripe, StripeElementChangeEvent } from '@stripe/stripe-js';
import * as React from 'react';
import { IPaymentIntentRequest, IPaymentIntentResponse, stripeApi } from 'src/Api/StripeApi';
import { OrderAction } from 'src/models/IDispatchStatus';
import { ToggleAlertAction } from 'src/state/contexts/landing/Actions';
import { OrderPaidAction, OrderStatusAction } from 'src/state/contexts/order/Actions';

export interface IOwnProps {
    elements: StripeElements | null,
    stripe: Stripe | null,
    orderId: string,
    invoice: number,
    orderPaid: (paid: boolean, stripePaymentConfirmationId?: string) => OrderPaidAction,
    orderStatusChange: (action: OrderAction, variant: Variant, show: boolean) => OrderStatusAction,
    handleAlert: (text: string, variant?: Variant, timeout?: number) => ToggleAlertAction
}

export interface IOwnState {
    clientSecret: string,
    disabled: boolean,
    processing: boolean
}

export class CheckoutForm extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            clientSecret: "",
            disabled: true,
            processing: false
        };
    }

    public componentDidMount() {
        this.createPaymentIntent();
    }

    public render() {
        const { processing, disabled } = this.state;

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
                <button disabled={processing || disabled} id="submit">
                    <span id="button-text">
                        {processing ? <div className="spinner" id="spinner" />  : "Pay" }
                    </span>
                </button>
            </form>
        );
    }

    private handleChange = async (event: StripeElementChangeEvent) => {
        this.setState({ disabled: event.empty })

        if (event.error) {
            this.props.handleAlert(event.error.message, Variant.Danger) 
        }
    }

    private createPaymentIntent = () => {
        if (this.props.orderId !== undefined) {
            const paymentIntentRequest : IPaymentIntentRequest = {
                orderId: this.props.orderId,
                invoice: this.props.invoice
            }

            stripeApi.paymentIntent(paymentIntentRequest)
                .then(r => this.createPaymentIntentSuccess(r))
        } 
    }    

    private createPaymentIntentSuccess = (response: IPaymentIntentResponse) => {
        this.setState({ clientSecret: response.clientSecret });

        if (!response.status) {
            this.props.orderStatusChange(OrderAction.PaymentIntent, Variant.Danger, true)
        }
        else{
            this.props.orderStatusChange(OrderAction.PaymentIntent, Variant.Success, false)
        }
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
            this.setState({ processing: false })
            this.props.handleAlert(`Payment failed ${payload.error.message}`, Variant.Danger) 
            this.props.orderPaid(false);
            this.props.orderStatusChange(OrderAction.Payment, Variant.Danger, true);
        } 
        else{
            this.setState({ processing: false })
            this.props.orderStatusChange(OrderAction.Payment, Variant.Success, true);
            this.props.orderPaid(true, payload.paymentIntent?.id);
            this.props.handleAlert("Payment successful", Variant.Success) 
        }
    };
}

