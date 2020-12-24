import { FormElement, FormElementType } from '@appology/react-components';
import * as React from 'react'
import { Modal } from 'react-bootstrap';
import Button from 'react-bootstrap/Alert';
import { FaCalendarCheck } from 'react-icons/fa';
import Select from 'react-select';
import { api } from 'src/Api/Api';
import { IEvent, IEventDTO } from 'src/models/IEvent';
import { ITag } from 'src/models/ITag';
import { IUserCalendar } from 'src/models/IUserCalendar';

interface IOwnState {
    show: boolean,
    loadingTags: boolean,
    userTags: ITag[],
    cronofyReady: boolean,
    event: IEventDTO,
    selectedCalendarIds: number[]
}

interface IOwnProps {
    saveEventChange: (show: boolean) => void,
    userId: string,
    event?: IEvent,
    userCalendars: IUserCalendar[]
}

export class SaveEvent extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            show: true,
            loadingTags: false,
            userTags: [],
            selectedCalendarIds: [],
            cronofyReady: false,
            event: {
                id: "",
                title: "",
                startStr: "",
                endStr: "",
                allDay: false,
                calendarId: "",
                tagId: "",
                description: "",
                tentative: false,
                eventUid: "",
                alarm: ""
            }
        };
    }

    public componentDidMount = () => {
        this.getUserTags();
    }

    public render() {
        return (
            <Modal show={this.state.show} onHide={this.handleClose}>
                <div className="modal-bg">
                    <Modal.Header closeButton={true}>
                        <Modal.Title>
                            <FaCalendarCheck /> Save Event 
                            {/* In calendar{this.props.selectedUserCalendars.length > 1 ? "s" : ""}
                            {
                                this.props.selectedUserCalendars.map((uc, idx) => (
                                    <div key={idx} className="badge badge-info text-small">{uc.name}</div>
                                ))
                            } */}
                        </Modal.Title>
                    </Modal.Header>
                    <Modal.Body style={{marginLeft: 10}}>
                        <span className="label-input100">Calendars</span>
                        <Select 
                            options={this.props.userCalendars.map(o => ({value: o.id,label: o.name}))}
                            placeholder="Calendars"
                            isMulti={true}
                            defaultValue={this.props.userCalendars.filter(uc => uc.selected).map(o => ({value: o.id,label: o.name}))}
                            onChange={(selected: any)  => this.handleCalendarsChange(selected.value)} />

                        <input type="hidden" id="hdEventID" value="0" />

                        <FormElement 
                            label="Tag"
                            icon="&#xf188;"
                            elementType={FormElementType.Select} 
                            selectorOptions={this.state.userTags}
                            onSelectChange={this.handleTagChange} 
                            loading={this.state.loadingTags}
                            required={true} 
                            disabled={this.state.loadingTags} />

                        <FormElement 
                            label="Start Date"
                            icon="&#xf337;"
                            elementType={FormElementType.Date} 
                            onInputChange={this.handleChange} 
                            required={true}  
                        />
                        <div className="form-group">
                            <div className="checkbox checkbox-inline" style={{display: "inline", marginRight: 10}}>
                                <label><input type="checkbox" id="chkIsFullDay" defaultChecked={false} onChange={this.handleChange} />  Full Day</label>
                            </div>
                            <div className="checkbox checkbox-inline" style={{display: "inline"}}>
                                <label><input type="checkbox" id="chkTentative" defaultChecked={false} onChange={this.handleChange} />  Tentative</label>
                            </div>
                        </div>
                        <FormElement 
                            label="End Date"
                            icon="&#xf337;"
                            elementType={FormElementType.Date} 
                            onInputChange={this.handleChange} 
                            required={true} 
                            styles={{display: "none"}} 
                        />
                        <FormElement 
                            label="Event Details"
                            icon="&#xf1f7;"
                            elementType={FormElementType.Textarea} 
                            onTextAreaChange={this.handleDetailsChange} 
                            textAreaRows={4}
                        />
                        {
                            this.state.cronofyReady ?
                                <FormElement 
                                    label="Set alarm in minutes seperated by commas"
                                    icon="&#xf32d;"
                                    elementType={FormElementType.Text} 
                                    onInputChange={this.handleChange} 
                                />
                            : null
                        }
                    </Modal.Body>
                    <Modal.Footer>
                        <Button variant="secondary" onClick={this.handleClose}>Close</Button>
                        <Button variant="primary" type="submit" onClick={this.handleSave}>Save</Button>
                    </Modal.Footer>
                </div>
            </Modal>


        );
    }

    private handleCalendarsChange = (calendarId: number) => {
        let selectedCalendars = [...this.state.selectedCalendarIds, calendarId];

        if (this.state.selectedCalendarIds.includes(calendarId)) {
            selectedCalendars = selectedCalendars.filter(uc => uc !== calendarId);
        } 

        if (selectedCalendars.length !== 0) {
            this.setState({
                selectedCalendarIds: selectedCalendars
            })
        }
    }

    private handleChange(event: React.ChangeEvent<HTMLInputElement>): void {
        this.setState({
            event: { ...this.state.event,    
                [event.target.name]: event.target.value    
            }
        })
    }

    private handleDetailsChange(description: React.ChangeEvent<HTMLTextAreaElement>): void {
        this.setState({
            event: { ...this.state.event,    
                description: description.target.value   
            }
        })
    }

    private handleTagChange = (tag: React.ChangeEvent<HTMLSelectElement>) => {
        this.setState({
            event: { ...this.state.event,    
                tagId: tag.target.value
            }
        })
    }

    private getUserTags = () => {
        this.setState({ loadingTags: true})


        api.userTags()
            .then(t => this.userTagsSuccess(t.userTags, t.cronofyReady));
    }

    private userTagsSuccess = (userTags: ITag[], cronofyReady: boolean) => {
        this.setState({ 
            loadingTags: false,
            cronofyReady: cronofyReady,
            userTags: userTags
        })
    }

    private handleClose = () => {
        this.setState({ show: false })
        this.props.saveEventChange(false);
    }

    private handleSave = () => {
        alert(JSON.stringify(this.state.event))
    }


}