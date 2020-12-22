import { ICustomer } from 'src/models/ICustomer';

export const customerSearchTxt = (customer: ICustomer): string => {
	return `${customer.firstName} ${customer.lastName} @ ${customer.address1} ${customer.postcode}`;
}

export const rootUrl: string = process.env.NODE_ENV === "development" ? "http://localhost:53822" : window.location.origin;
export const appUrl: string = "http://localhost:3000";
