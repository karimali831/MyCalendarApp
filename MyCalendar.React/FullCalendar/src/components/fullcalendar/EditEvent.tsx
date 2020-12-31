import * as React from 'react';
import { Modal, OverlayTrigger, Tooltip } from 'react-bootstrap';
import { FaBell, FaClock, FaEdit, FaEnvelope, FaExclamation, FaInfo, FaTag, FaTrashAlt } from 'react-icons/fa';
import * as moment from 'moment';
import { IEventDTO } from 'src/models/IEvent';

interface IOwnState {
    show: boolean,
    showSaveEvent: boolean
}

interface IOwnProps {
    event: IEventDTO,
    reminders?: string,
    onCancelChange: () => void,
    onEditChange: () => void,
    onDeleteChange: (eventId: string) => void
}


export class EditEvent extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            show: true,
            showSaveEvent: false
        };
    }

    public render() {
        const startDate = moment(this.props.event.startStr);
        const endDate = moment(this.props.event.endStr)

        return (
            <Modal show={this.state.show} onHide={this.handleClose}>
                <div className="modal-bg">
                    <Modal.Header closeButton={true}>
                        <div className="edit-event-actions">
                            {this.props.event.reminder ? "Edit Reminder" : "Edit Event"}
                            <OverlayTrigger placement="bottom" overlay={ <Tooltip>Edit event</Tooltip>}>
                                <FaEdit cursor="pointer" onClick={this.handleEditChange} className="event-action" />
                            </OverlayTrigger>
                            <OverlayTrigger placement="bottom" overlay={ <Tooltip>Delete event</Tooltip>}>
                                <FaTrashAlt cursor="pointer" onClick={this.handleDeleteChange}  className="event-action" />
                            </OverlayTrigger>
                            <OverlayTrigger placement="bottom" overlay={ <Tooltip>Invite buddys</Tooltip>}>
                                <FaEnvelope cursor="pointer" className="event-action"/>
                            </OverlayTrigger>
                            {
                                this.props.event.tentative ?
                                    <>
                                        <OverlayTrigger placement="bottom" overlay={ <Tooltip>This event is tentative</Tooltip>}>
                                            <FaExclamation className="event-action" /> 
                                        </OverlayTrigger>
                                    </>
                                : null
                            }
                        </div>
                    </Modal.Header>
                    <Modal.Body style={{marginLeft: 10}}>
                        <FaTag /> <strong>{this.props.event.title}</strong> 
                        <div className="event-info">
                            {
                                startDate.format("DD-MM-YYYY") === endDate.format("DD-MM-YYYY") || this.props.event.reminder ? 
                                    `${startDate.format("MMMM Do YYYY")} ${startDate.format("HH:mm")} ${!this.props.event.reminder ? `- ${endDate.format("HH:mm")}` : ""}` :
                                    `${startDate.format("MMMM Do YYYY HH:mm")} ${!this.props.event.allDay ? `to ${endDate.format("MMM Do YYYY HH:mm")}` : ""}`
                            }
                        </div>
                        {
                            !this.props.event.reminder ?
                                <>
                                    <FaClock /> <strong>Duration</strong>
                                    <div className="event-info">
                                        {this.props.event.allDay ? "All day" : this.props.event.duration}
                                    </div>
                                </>
                            :
                            null
                        }
                        {
                            this.props.event.alarm !== null && this.props.event.alarm !== "" ?
                                <>
                                    <FaBell /> <strong>Reminders</strong>
                                    <div className="event-info">
                                        {this.props.event.alarm} minutes prior to start
                                    </div>
                                </>
                            : null
                        }
                        {
                            this.props.event.description !== null && this.props.event.description !== "" && !this.props.event.reminder ?
                                <>
                                    <FaInfo /> <strong>Description</strong>
                                    <div className="event-info">
                                        {this.props.event.description}
                                    </div>
                                </>
                            : null
                        }
                    </Modal.Body>
                </div>
            </Modal>

        );
    }

    private handleEditChange = () => {
        this.props.onEditChange()
        this.setState({ show: false })
    }

    private handleDeleteChange = () => {
        if (confirm("Are you sure you?"))
        {
            this.props.onDeleteChange(this.props.event.id)
            this.setState({ show: false })
        }
    }

    private handleClose = () => {
        this.props.onCancelChange();
        this.setState({ show: false })
    }

}