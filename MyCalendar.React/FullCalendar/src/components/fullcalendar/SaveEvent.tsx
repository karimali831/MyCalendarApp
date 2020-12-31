import { FormElement, FormElementType, ToggleSwitch, intIsNullOrEmpty, objIsNullOrEmpty } from '@appology/react-components';
import * as React from 'react'
import { Modal } from 'react-bootstrap';
import Button from 'react-bootstrap/Alert';
import Select from 'react-select';
import { api } from 'src/Api/Api';
import { IEvent, IEventDTO, IEventSelect } from 'src/models/IEvent';
import { ITag } from 'src/models/ITag';
import { ISelect, IUserCalendar } from 'src/models/IUserCalendar';
import * as moment from 'moment';

interface IOwnState {
    show: boolean,
    loadingTags: boolean,
    userTags: ITag[],
    cronofyReady: boolean,
    event: IEventDTO
}

interface IOwnProps {
    userId: string,
    eventSelect?: IEventSelect,
    userCalendars: IUserCalendar[],
    onSaveChange: (loading: boolean, event?: IEvent) => void,
    onCancelChange: () => void
}

export class SaveEvent extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);
        const startTime = Number(`${moment().add(1, 'h').hours()}`);
        const endTime = Number(`${moment().add(3, 'h').hours()}`);

        this.state = {
            show: true,
            loadingTags: false,
            userTags: [],
            cronofyReady: false,
            event: this.props.eventSelect?.event !== undefined ? this.props.eventSelect.event :  {
                id: "",
                calendarId: this.props.userCalendars.filter(uc => uc.selected).map(o => o.id)[0],
                reminder: false,
                title: "",
                startStr: moment(this.props.eventSelect?.dateStart?.setHours(startTime)).format('YYYY-MM-DD[T]HH:00'),
                endStr: moment(this.props.eventSelect?.dateEnd?.setHours(endTime)).subtract(1, "days").format('YYYY-MM-DD[T]HH:00'),
                allDay: false,
                tagId: "",
                description: "",
                tentative: false,
                eventUid: null,
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
                    <Modal.Header closeButton={true}>Save Event</Modal.Header>
                
                    <Modal.Body style={{marginLeft: 10}}>
                        <div className="m-b-23">
                            <span className="label-input100">Calendar</span>
                            <Select 
                                options={this.props.userCalendars.map(o => ({value: o.id,label: o.name}))}
                                placeholder="Select Calendar"
                                isMulti={false}
                                defaultValue={this.props.userCalendars.filter(uc => uc.id === this.state.event.calendarId).map(o => ({value: o.id,label: o.name}))[0]}
                                onChange={(value: ISelect) => this.handleCalendarsChange(value)} />
                        </div>
                        <div className="m-b-23">
                            <ToggleSwitch inline={true} id="reminder" name="Reminder" checked={this.state.event.reminder} onChange={(value: boolean) => this.handleReminderChange(value)} />
                            {
                                !this.state.event.reminder ?
                                    <>
                                        <ToggleSwitch inline={true} id="allday" name="All day" checked={this.state.event.allDay} onChange={(v: boolean) => this.handleFullDayChange(v)} />
                                        <ToggleSwitch inline={true} id="tentative" name="Tentative" checked={this.state.event.tentative} onChange={(v: boolean) => this.handleTentativeChange(v)} />
                                    </>
                                : null
                            }
                        </div>
                        <input type="hidden" id="hdEventID" value="0" />

                        {
                            !this.state.event.reminder ? 
                                <>
                                    <FormElement 
                                        label="Tag"
                                        id="tagId"
                                        icon="&#xf188;"
                                        elementType={FormElementType.Select} 
                                        selected={this.props.eventSelect?.event?.tagId}
                                        selectorOptions={this.state.userTags}
                                        onSelectChange={this.handleTagChange} 
                                        loading={this.state.loadingTags}
                                        required={true} 
                                        disabled={this.state.loadingTags} 
                                    />
                                    <FormElement 
                                        label="Start Date"
                                        id="startStr"
                                        defaultValue={this.state.event.startStr}
                                        icon="&#xf337;"
                                        elementType={FormElementType.Date} 
                                        onInputChange={this.handleChange} 
                                        required={true}  
                                    />

                                    {
                                        !this.state.event.allDay ?
                                            <FormElement 
                                                label="End Date"
                                                defaultValue={this.state.event.endStr}
                                                id="endStr"
                                                icon="&#xf337;"
                                                elementType={FormElementType.Date} 
                                                onInputChange={this.handleChange} 
                                                required={true}  
                                            />
                                        : null
                                    }
            
                                    <FormElement 
                                        label="Event Details"
                                        value={this.state.event.description}
                                        id="description"
                                        icon="&#xf1f7;"
                                        elementType={FormElementType.Textarea} 
                                        onTextAreaChange={this.handleDetailsChange} 
                                        textAreaRows={4}
                                    />
                                </> : 
                                <>
                                    <FormElement 
                                        label="Title"
                                        id="title"
                                        defaultValue={this.state.event.title}
                                        icon="&#xf1f7;"
                                        elementType={FormElementType.Text} 
                                        onInputChange={this.handleChange} 
                                    />
                                    <FormElement 
                                        label="Date & Time"
                                        defaultValue={this.state.event.startStr}
                                        id="startStr"
                                        icon="&#xf337;"
                                        elementType={FormElementType.Date} 
                                        onInputChange={this.handleChange} 
                                        required={true}  
                                    />
                                </>
                        }
                        {
                            this.state.cronofyReady ?
                                <FormElement 
                                    label="Set alarm in minutes seperated by commas"
                                    id="alarm"
                                    value={this.state.event.alarm}
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

    private handleReminderChange = (value: boolean) => {
        this.setState({
            event: { ...this.state.event,    
                reminder: value    
            }
         })
    }

    private handleFullDayChange = (value: boolean) => {
        this.setState({
            event: { ...this.state.event,    
                allDay: value    
            }
        })
    }

    private handleTentativeChange = (value: boolean) => {
        this.setState({
            event: { ...this.state.event,    
                tentative: value    
            }
        })
    }

    private handleCalendarsChange = (selectedCalendar: ISelect) => {
        this.setState({
            event: { ...this.state.event,    
                calendarId: selectedCalendar.value
            }
        })
        
    }

    private handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {

        this.setState({
            event: { ...this.state.event,    
                [e.target.name]: e.target.value    
            }
        })
    }

    private handleDetailsChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
        this.setState({
            event: { ...this.state.event,    
                description: e.target.value   
            }
        })
    }

    private handleTagChange = (tag: React.ChangeEvent<HTMLSelectElement>) => {
        this.setState({
            event: { ...this.state.event,    
                tagId: tag.target.value
            }
        })

        if (this.state.cronofyReady) {
            api.alarmInfo(tag.target.value)
                .then(a => this.alarmInfoSuccess(a));
        }
    }

    private alarmInfoSuccess = (alarmInfo: string) => {
        this.setState({
            event: { ...this.state.event,    
                alarm: alarmInfo
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
        this.props.onCancelChange();
        this.setState({ show: false })
    }

    private handleSave = () => {
        // validation
        const startDate = moment(this.state.event.startStr);
        let errorMsg = "";
        
        if (!startDate.isValid())
        {
            errorMsg = "Start date is not a valid input";
        }

        if (!this.state.event.allDay && !this.state.event.reminder)
        {
            const endDate = moment(this.state.event.endStr);

            if (!endDate.isValid())
            {
                errorMsg = "End date is not a valid input";
            }

            if (startDate > endDate)
            {
                errorMsg = "End date must be ahead of start date";
            }
        }

        if (this.state.event.reminder) {
            if (this.state.event.title === "") {
                errorMsg = "Reminder title cannot be empty";
            }

            if (moment() > startDate) {
                errorMsg = "Reminder date cannot be in the past";
            }
        }
        else{
            if (this.state.event.description === "" && this.state.event.tagId === "") {
                errorMsg = "Tag and description cannot both be empty" 
            }
        }

        if (intIsNullOrEmpty(this.state.event.calendarId)) {
            errorMsg = "You must select a calendar";
        }

        if (errorMsg !== "") {
            alert (errorMsg);
        }
        else{

            this.setState({ show: false })
            this.props.onSaveChange(true);

            api.saveEvent(this.state.event)
                .then(s => this.eventSaveSuccess(s));
        }
    }

    private eventSaveSuccess = (event: IEvent) => {
        if (objIsNullOrEmpty(event)) {
            alert("There was an issue saving the event, please try again");
        }
        else{
            this.props.onSaveChange(false, event);
        }
    }
}