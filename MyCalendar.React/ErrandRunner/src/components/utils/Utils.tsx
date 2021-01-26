import { Variant } from '@appology/react-components';
import * as React from 'react';
import { IStakeholder } from 'src/models/IStakeholder';
import { AlertModal } from './Alert';

export const stakeholderSearchTxt = (stakeholder: IStakeholder): string => {
	return `${stakeholder.firstName} ${stakeholder.lastName} @ ${stakeholder.address1} ${stakeholder.postcode}`;
}

export const showAlert = (msg: string, variant?: Variant) => <AlertModal alertMsg={msg} alertVariant={variant} alert={true} />
export const rootUrl: string = process.env.NODE_ENV === "development" ? "http://localhost:53822" : window.location.origin;
export const appUrl: string = "http://localhost:3000";
