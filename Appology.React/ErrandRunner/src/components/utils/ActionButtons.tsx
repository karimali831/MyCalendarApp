import { Load } from '@appology/react-components';
import * as React from 'react';
import Button from 'react-bootstrap/Button';

interface IOwnProps {
    saving?: boolean,
    deleting?: boolean,
    icon?: JSX.Element,
    style?: React.CSSProperties,
    value?: string,
    onSaveClick?: React.MouseEventHandler<HTMLButtonElement>,
    onDeleteClick?: React.MouseEventHandler<HTMLButtonElement>
}

export const SaveButton: React.FC<IOwnProps> = (props) => {
    return (
        <Button style={props.style} type="submit" disabled={props.saving} onClick={props.onSaveClick}>
            {props.icon} {props.saving ? <>Saving... <Load inlineDisplay={true} smallSize={true} /></> : props.value ?? "Save"}
        </Button>
    )
}

export const DeleteButton: React.FC<IOwnProps> = (props) => {
    return (
        <Button style={props.style} type="submit" variant="danger" disabled={props.deleting} onClick={props.onDeleteClick}>
            {props.icon} {props.deleting ? <>Deleting... <Load inlineDisplay={true} smallSize={true} /></> : props.value ?? "Delete"}
        </Button>
    )
}