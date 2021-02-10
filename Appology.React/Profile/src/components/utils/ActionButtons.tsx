import { Load } from '@appology/react-components';
import * as React from 'react';
import Button from 'react-bootstrap/Button';

interface IOwnProps {
    saving?: boolean,
    deleting?: boolean,
    onSaveClick?: React.MouseEventHandler<HTMLButtonElement>,
    onDeleteClick?: React.MouseEventHandler<HTMLButtonElement>
}

export const SaveButton: React.FC<IOwnProps> = (props) => {
    return (
        <Button type="submit" disabled={props.saving} onClick={props.onSaveClick}>
            {props.saving ? <>Saving... <Load inlineDisplay={true} smallSize={true} /></> : "Save"}
        </Button>
    )
}

export const DeleteButton: React.FC<IOwnProps> = (props) => {
    return (
        <Button type="submit" variant="danger" disabled={props.deleting} onClick={props.onDeleteClick}>
            {props.deleting ? <>Deleting... <Load inlineDisplay={true} smallSize={true} /></> : "Delete"}
        </Button>
    )
}