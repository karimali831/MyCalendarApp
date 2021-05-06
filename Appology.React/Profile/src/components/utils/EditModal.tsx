import { Variant } from '@appology/react-components';
import * as React from 'react';
import { Modal } from 'react-bootstrap';
import Button from 'react-bootstrap/Button';
import { ActionButton } from './ActionButtons';

export interface IOwnState {
}

export interface IOwnProps {
    title: string,
    content: JSX.Element[],
    titleIcon?: JSX.Element
    delete?: boolean,
    deleteTxt?: string,
    closeTxt?: string,
    saveTxt?: string,
    save: boolean,
    show: boolean, 
    handleClose: () => void,
    onDelete?: React.MouseEventHandler<HTMLButtonElement>,
    onSave?: React.MouseEventHandler<HTMLButtonElement>
}

export class EditModal extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
        };
    }

    public render() {
        return (
            <Modal show={this.props.show} onHide={this.props.handleClose}>
                {
                    this.props.title !== "" &&
                    <Modal.Header closeButton={true}>
                        <Modal.Title>{this.props.titleIcon} {this.props.title}</Modal.Title>
                    </Modal.Header>
                }
                <Modal.Body>
                    {this.props.content}
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={this.props.handleClose}>{this.props.closeTxt ?? "Close"}</Button>
                    {this.props.delete !== undefined && <ActionButton variant={Variant.Danger} loading={this.props.delete} onClick={this.props.onDelete} value={this.props.deleteTxt ?? "Delete"} />}
                    <ActionButton loading={this.props.save} onClick={this.props.onSave} value={this.props.saveTxt} />
                </Modal.Footer>
            </Modal>
        );
    }
}