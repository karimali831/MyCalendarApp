import { IStakeholder } from 'src/models/IStakeholder';

export const stakeholderSearchTxt = (stakeholder: IStakeholder): string => {
	return `${stakeholder.firstName} ${stakeholder.lastName} @ ${stakeholder.address1} ${stakeholder.postcode}`;
}

export const rootUrl: string = process.env.NODE_ENV === "development" ? "http://localhost:53822" : window.location.origin;
export const appUrl: string = "http://localhost:3000";
