import * as React from 'react';
import { Modal } from 'react-bootstrap'
import { SaveButton, DeleteButton } from './ActionButtons';

export interface IOwnState {

}

export interface IOwnProps {
    title: string,
    content: JSX.Element[],
    titleIcon?: JSX.Element
    delete?: boolean,
    save: boolean,
    onDelete?: React.MouseEventHandler<HTMLButtonElement>,
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
            <Modal.Dialog style={{ position: "relative", left: 0, top: 0 }}>
                {
                    this.props.title !== "" &&
                    <Modal.Header>
                        <Modal.Title>{this.props.titleIcon} {this.props.title}</Modal.Title>
                    </Modal.Header>
                }

                <Modal.Body>
                    {this.props.content}
                </Modal.Body>

                <Modal.Footer>
                    {this.props.delete !== undefined && <DeleteButton deleting={this.props.delete} onDeleteClick={this.props.onDelete} />}
                    <SaveButton saving={this.props.save} onSaveClick={this.props.onSave} />
                </Modal.Footer>
            </Modal.Dialog>
        );
    }

}