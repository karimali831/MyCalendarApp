import * as React from 'react';
import { ICustomer } from 'src/models/ICustomer';
export const capitalize = (s: string) => {
	if (typeof s !== 'string') { return '' }
	return s.charAt(0).toUpperCase() + s.slice(1)
}

export const priceFormatter = (cell: any, row: any) => {
    // `<i class='glyphicon glyphicon-gbp'></i> ${cell}`;
    return `£${cell}`;
}

export const cleanText = (text: string) => {
	return text.toString().replace(/([A-Z])/g, ' $1').trim()
}

export const boolHighlight = (bool: boolean) => {
	return <span className={"label label-" + (bool ? "success" : "danger")}>{bool ? "Yes" : "No"}</span>
}

export const customerSearchTxt = (customer: ICustomer): string => {
	return `${customer.firstName} ${customer.lastName} @ ${customer.address1} ${customer.postcode}`;
}

export const rootUrl: string = process.env.NODE_ENV === "development" ? "http://localhost:53822" : window.location.origin;
export const appUrl: string = "http://localhost:3000";
