import { Variant } from '@appology/react-components/src/Enums/Variant';
import * as React from 'react';
import { AlertModal } from './Alert';


export const showAlert = (msg: string, variant?: Variant) => <AlertModal alertMsg={msg} alertVariant={variant} alert={true} />
export const rootUrl: string = process.env.NODE_ENV === "development" ? "http://localhost:53822" : window.location.origin;
export const appUrl: string = "http://localhost:3000";
