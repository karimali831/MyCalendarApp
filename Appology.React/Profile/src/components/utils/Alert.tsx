import { Variant } from '@appology/react-components/src/Enums/Variant';
import { Modal } from '@appology/react-components/src/Modal/Modal'
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