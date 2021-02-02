import { Variant, Modal } from '@appology/react-components';
import * as React from 'react';

export interface IOwnProps {
    alert?: boolean | true,
    alertMsg: string,
    alertVariant?: Variant | Variant.Success
}

export const AlertModal: React.FC<IOwnProps> = (props) => {
    return (
        props.alert ?
            <Modal show={true} text={props.alertMsg} variant={props.alertVariant} />
        : null
    )
}