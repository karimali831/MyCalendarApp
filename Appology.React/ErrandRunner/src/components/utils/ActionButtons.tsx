import { Load, Variant } from '@appology/react-components';
import * as React from 'react';
import Button from 'react-bootstrap/Button';

interface IOwnProps {
    loading: boolean,
    icon?: JSX.Element,
    value?: string,
    variant?: Variant,
    inlineDisplay?: boolean,
    disabled?: boolean,
    onClick?: React.MouseEventHandler<HTMLButtonElement>
}

export const variant = (props: IOwnProps) => props.variant !== undefined ? Variant[props.variant] : Variant[Variant.Primary]

export const ActionButton: React.FC<IOwnProps> = (props) => {
    return (
        <Button type="submit" variant={variant(props).toLowerCase()} disabled={props.loading || props.disabled} onClick={props.onClick}>
             {props.loading ? <Load inlineDisplay={props.inlineDisplay ?? true} smallSize={true} /> : <>{props.icon} {props.value ?? "Save"}</>}
        </Button>
    )
}