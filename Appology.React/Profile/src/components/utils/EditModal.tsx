import * as React from 'react';
import { Modal } from 'react-bootstrap'
import { ActionButton } from './ActionButtons';
import Button from 'react-bootstrap/Button'
import { Variant } from '@appology/react-components';

export interface IOwnState {
}

export interface IOwnProps {
    title: string,
    content: JSX.Element[],
    titleIcon?: JSX.Element
    delete?: boolean,
    save: boolean,
    show: boolean, 
    handleClose: () => void,
    onDelete: React.MouseEventHandler<HTMLButtonElement>,
    onSave: React.MouseEventHandler<HTMLButtonElement>
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
                    <Button variant="secondary" onClick={this.props.handleClose}>Close</Button>
                    {this.props.delete !== undefined && <ActionButton variant={Variant.Danger} loading={this.props.delete} onClick={this.props.onDelete} value="Delete" />}
                    <ActionButton loading={this.props.save} onClick={this.props.onSave} />
                </Modal.Footer>
            </Modal>
        );
    }
}