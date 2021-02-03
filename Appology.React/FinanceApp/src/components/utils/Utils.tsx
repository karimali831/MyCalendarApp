import * as React from 'react';
import { CategoryType } from 'src/enums/CategoryType';
import { DateFrequency } from 'src/enums/DateFrequency';
import { Priority } from 'src/enums/Priority';
import { PaymentStatus } from 'src/enums/PaymentStatus';
import Badge from 'react-bootstrap/Badge'

export const shuffle = (a: any) => {
    for (let i = a.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [a[i], a[j]] = [a[j], a[i]];
    }
    return a;
}

export const intToOrdinalNumberString = (cell: any, row: any) => {
	cell = Math.round(cell);
	const numString = cell.toString();

	// If the ten's place is 1, the suffix is always "th"
	// (10th, 11th, 12th, 13th, 14th, 111th, 112th, etc.)
	if (Math.floor(cell / 10) % 10 === 1) {
		return numString + "th";
    }
    
    // If its 0
    if (cell === 0) {
        return "";
    }

	// Otherwise, the suffix depends on the one's place as follows
	// (1st, 2nd, 3rd, 4th, 21st, 22nd, etc.)
	switch (cell % 10) {
		case 1: return numString + "st";
		case 2: return numString + "nd";
		case 3: return numString + "rd";
		default: return numString + "th";
	}
}

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

export const cleanText2 = (text: string) => {
	return text.replace(/[^A-Z0-9]/ig, "").toLowerCase();
}

export const boolHighlight = (bool: boolean) => {
	return <Badge variant={bool ? "success" : "danger"}>{bool ? "Yes" : "No"}</Badge>
}

export const monthNames = ["January", "February", "March", "April", "May","June","July", "August", "September", "October", "November","December"]

export const paymentStatus = (status: PaymentStatus, daysUntilDue: number) => {
	switch (status) {
		case PaymentStatus.Paid:
			return <Badge variant="success">{PaymentStatus[PaymentStatus.Paid]}</Badge>
		case PaymentStatus.Upcoming:
			return <Badge variant="warning">{PaymentStatus[PaymentStatus.Upcoming]} ({daysUntilDue} days)</Badge>
		case PaymentStatus.Late:
			return <Badge variant="danger">{PaymentStatus[PaymentStatus.Late]} ({daysUntilDue} days)</Badge>
		case PaymentStatus.Unknown:
			return <Badge variant="danger">{PaymentStatus[PaymentStatus.Unknown]}</Badge>
		case PaymentStatus.DueToday:
			return <Badge variant="danger">{PaymentStatus[PaymentStatus.DueToday]} ({daysUntilDue} days)</Badge>
		case PaymentStatus.Ended:
			return <Badge variant="secondary">{PaymentStatus[PaymentStatus.Ended]}</Badge>
		default:
			return ""
	}
}

export const priorityBadge = (priority: string, label: string = "") => {
	switch (priority) {
		case Priority[Priority.Low]:
			return <Badge variant="info">{label !== "" ? label : Priority[Priority.Low]}</Badge>
		case Priority[Priority.Medium]:
			return <Badge variant="warning">{label !== "" ? label : Priority[Priority.Medium]}</Badge>
		case Priority[Priority.High]:
			return <Badge variant="danger">{label !== "" ? label : Priority[Priority.High]}</Badge>
		default:
			return <Badge variant="info">{label !== "" ? label : Priority[Priority.Low]}</Badge>
	}
}


export const rootUrl: string = process.env.NODE_ENV === "development" ? "http://localhost:53822" : window.location.origin;
export const appUrl: string = "http://localhost:3000";
export const appPathUrl: string = "/finance/app";
export const spendingsForFinance = (id: number) => `${appPathUrl}/spendings/${id}/0/1/true/false/null/null`;

export const SummaryFilteredList =
	(categoryType: CategoryType, catId?: number, frequency?: DateFrequency, interval?: number, isFinance?: boolean, isSecondCat?: boolean, fromDate?: string | null, toDate?: string | null) => `${appPathUrl}/${CategoryType[categoryType]}/${catId}/${frequency}/${interval}/${categoryType === CategoryType.Spendings ? isFinance + "/" : ""}${isSecondCat}/${fromDate}/${toDate}`;

export const distinctValues = (value: string, index: number, self: any) =>
	self.indexOf(value) === index;
