import { rootUrl } from 'src/components/utils/Utils';

export class StripeApi {
    public rootUrl: string = `${rootUrl}/api/stripe`;


    public paymentIntent = async (request: IPaymentIntentRequest): Promise<IPaymentIntentResponse> => {
        return fetch(`${this.rootUrl}/create-payment-intent`, {
            method: "POST",
            body: JSON.stringify(request),
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin',
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as IPaymentIntentResponse);
    }
}

export const stripeApi = new StripeApi();

export interface IPaymentIntentRequest {
    orderId: string,
    invoice: number
}

export interface IPaymentIntentResponse {
    clientSecret: string,
    status: boolean
}
