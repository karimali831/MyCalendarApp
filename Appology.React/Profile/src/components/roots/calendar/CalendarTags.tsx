import { InputElement, SelectElement, ToggleSwitch, Variant } from '@appology/react-components';
import IBaseModel from '@appology/react-components/dist/SelectionRefinement/IBaseModel';
import IUserType from '@appology/react-components/dist/UserTypes/IUserType';
import * as React from 'react';
import Alert from 'react-bootstrap/Alert';
import Button from 'react-bootstrap/Button';
import Form from 'react-bootstrap/Form';
import InputGroup from 'react-bootstrap/InputGroup';
import Table from 'react-bootstrap/Table';
import { FaMinus, FaPlus, FaTimes } from 'react-icons/fa';
import { api } from 'src/Api/Api';
import { ActionButton } from 'src/components/utils/ActionButtons';
import { EditModal } from 'src/components/utils/EditModal';
import { DayOfWeek } from 'src/Enums/DayOfWeek';
import { TimeFrequency } from 'src/Enums/TimeFrequency';
import { IUserTag } from 'src/models/IUserTag';

export interface IOwnState {
    userTags: IUserTag[],
    userTagGroups: IUserType[],
    saving: boolean,
    editActivityHubId?: number,
    activityHubTargetValueError: string,
    activityHubTargetFrequencyError: string,
    activityHubDayOfWeekError: string
}

export interface IOwnProps {
    userTags: IUserTag[],
    userTagGroups: IUserType[],
    showAlert: (txt: string, variant: Variant) => void
}


export class CalendarTags extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            userTags: this.props.userTags,
            userTagGroups: this.props.userTagGroups,
            saving: false,
            editActivityHubId: undefined,
            activityHubTargetValueError: "",
            activityHubTargetFrequencyError: "",
            activityHubDayOfWeekError: ""
        };
    }
    public render() {
        return (
            <>
                {
                    this.state.editActivityHubId !== undefined &&
                    <EditModal 
                        title="Activity Hub Settings"
                        content={this.activityHubSettings()}
                        show={true}
                        save={false}
                        delete={false}
                        saveTxt="Confirm"
                        deleteTxt="Disable"
                        closeTxt="Close"
                        onSave={() => this.activityHubConfirmSettings(this.state.editActivityHubId ?? 0)}
                        onDelete={() => this.enableActivityHubChange(this.state.editActivityHubId ?? 0, false)}
                        handleClose={() => this.activityHubConfirmSettings(this.state.editActivityHubId ?? 0)}
                    />
                }
                <Table responsive={true}>
                    <thead>
                        <tr>
                            <th>Label</th>
                            <th>Theme</th>
                            <th align="center">Activity Hub</th>
                            <th>Group</th>
                        </tr>
                    </thead>
                    <tbody>
                    {
                        this.state.userTags.map((x, i) => {
                            return (
                                <>
                                    <tr>
                                        <td width="30%">
                                            <Form.Control 
                                                name="name"
                                                value={x.name}
                                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => this.handleInputChange(e, i)} />
                                        </td>
                                        <td width="20%">
                                            <Form.Control 
                                                name="themeColor"
                                                type="color"
                                                value={x.themeColor}
                                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => this.handleInputChange(e, i)} />
                                        </td>
                                        <td width="10%" align="center">
                                            <ToggleSwitch  id={`ActivityHub${i}`} name="" checked={x.targetUnit !== "disable"} onChange={(checked: boolean) => this.enableActivityHubChange(i, checked)} />
                                        </td>
                                        <td width="40%">
                                            <InputGroup className="mb-2 mr-sm-2">
                                                <Form.Control 
                                                    as="select" 
                                                    name="typeID"
                                                    value={x.typeID}
                                                    onChange={(e: React.ChangeEvent<HTMLSelectElement>) => this.handleSelectChange(e, i)}>
                                                        <option key={0} value={0}>Select Group</option>
                                                        {this.state.userTagGroups.map(g => 
                                                            <option key={g.key} value={g.key}>{g.title}</option>
                                                        )}
                                                </Form.Control>
                                                <InputGroup.Prepend>                 
                                                    {
                                                        this.state.userTags.length !== 1 && 
                                                            <Button variant="danger" onClick={() => this.handleRemoveClick(i)}>
                                                                <FaMinus />
                                                            </Button>
                                                    }
                                                </InputGroup.Prepend>
                                            </InputGroup>
                                        </td>
                                    </tr>
                                    <tr>
                                        {
                                            this.state.userTags.length - 1 === i && 
                                            <>
                                                <td colSpan={2}>
                                                    <Button variant="success" onClick={this.handleAddClick}>
                                                        <FaPlus /> Add Item
                                                    </Button>
                                                </td>
                                                <td colSpan={2} align="right">
                                                    <ActionButton loading={this.state.saving} onClick={() => this.saveTags()} />
                                                </td>
                                            </>
                                        }
                                    </tr>
                                </>
                            );
                        })
                    }
                    </tbody>
                </Table>
            </>
        );
    }

    private activityHubConfirmSettings = (editActivityHubId: number) => {

        const activity : IUserTag = this.state.userTags[editActivityHubId]
        const value = activity.targetValue?.toString();
        const frequency = activity.targetFrequency?.toString();
 
        if (activity.startDayOfWeek.toString() === activity.endDayOfWeek.toString()) {
            this.setState({ 
                activityHubTargetFrequencyError: "",
                activityHubDayOfWeekError: "Week start and end days cannot be the same",
                activityHubTargetValueError: ""
            })
        }
        else if (value === "0" || !Number(value)) {
            this.setState({ 
                activityHubTargetFrequencyError: "",
                activityHubDayOfWeekError: "",
                activityHubTargetValueError: "You must select a number value greater than 0" 
            })
        }
        else if (!frequency) {
            this.setState({ 
                activityHubTargetValueError: "",
                activityHubDayOfWeekError: "",
                activityHubTargetFrequencyError: "You must select a target frequency" 
            })

        } else {
            this.handleClose();
        }
    
    }

    private handleClose = () => {
        this.setState({ 
            activityHubTargetValueError: "",
            activityHubTargetFrequencyError: "", 
            editActivityHubId: undefined 
        })
    }

    private activityHubSettings = (): JSX.Element[] => {

        const elements: JSX.Element[] = []
        const frequencies : IBaseModel[] = [];
        const days : IBaseModel[] = [];

        for (const value in Object.keys(TimeFrequency)) {
            if (typeof TimeFrequency[value] !== "string") {
                continue;
            }

            frequencies.push({ id: value, name: TimeFrequency[value] })
        }

        for (const value in Object.keys(DayOfWeek)) {
            if (typeof DayOfWeek[value] !== "string") {
                continue;
            }

            days.push({ id: value, name: DayOfWeek[value] })
        }

        if (this.state.editActivityHubId !== undefined) {

            const activity : IUserTag = this.state.userTags[this.state.editActivityHubId]

            elements.push(
                <>
                    {
                        this.state.activityHubTargetFrequencyError !== "" && 
                            <Alert variant="danger">
                                <FaTimes /> {this.state.activityHubTargetFrequencyError}
                            </Alert>
                    }
                    <SelectElement 
                        label="Target Frequency"
                        id="targetFrequency"
                        selectorName="Select Frequency"
                        icon="&#xf334;"
                        selected={activity.targetFrequency?.toString()}
                        selectorOptions={frequencies}
                        onSelectChange={(e: React.ChangeEvent<HTMLSelectElement>) => this.handleSelectChange(e, this.state.editActivityHubId ?? 0)} 
                    />
                    {
                        this.state.activityHubDayOfWeekError !== "" && 
                            <Alert variant="danger">
                                <FaTimes /> {this.state.activityHubDayOfWeekError}
                            </Alert>
                    }
                    <SelectElement 
                        label="Start Day of Week"
                        id="startDayOfWeek"
                        icon="&#xf32f;"
                        selected={activity.startDayOfWeek?.toString() ?? DayOfWeek[DayOfWeek.Monday]}
                        selectorOptions={days}
                        onSelectChange={(e: React.ChangeEvent<HTMLSelectElement>) => this.handleSelectChange(e, this.state.editActivityHubId ?? 0)} 
                    />
                    <SelectElement 
                        label="End Day of Week"
                        id="endDayOfWeek"
                        icon="&#xf32f;"
                        selected={activity.endDayOfWeek?.toString() ?? DayOfWeek[DayOfWeek.Sunday]}
                        selectorOptions={days}
                        onSelectChange={(e: React.ChangeEvent<HTMLSelectElement>) => this.handleSelectChange(e, this.state.editActivityHubId ?? 0)} 
                    />
                    {
                        this.state.activityHubTargetValueError !== "" && 
                            <Alert variant="danger">
                                <FaTimes /> {this.state.activityHubTargetValueError}
                            </Alert>
                    }
                    <InputElement
                        key={this.state.editActivityHubId}
                        label="Target Value"
                        icon="&#xf108;"
                        id="targetValue"
                        numberInput={true}
                        defaultValue={activity.targetValue?.toString() ?? "0"}
                        required={false}
                        onInputChange={(e: React.ChangeEvent<HTMLInputElement>) => this.handleInputChange(e, this.state.editActivityHubId ?? 0)}
                    />
                    <ToggleSwitch  
                        id="ActivityHubTargetByHours" 
                        name="Target by Hours" 
                        checked={activity.targetUnit === "hours"} 
                        onChange={(checked: boolean) => this.activityHubTargetByHoursChange(this.state.editActivityHubId ?? 0, checked, false, true)} 
                    />
                    <br />
                    {
                        activity.targetUnit !== "hours" &&
                        <>
                            <InputElement
                                key={this.state.editActivityHubId}
                                label="Target Unit"
                                icon="&#xf187;"
                                id="targetUnit"
                                defaultValue={activity.targetUnit}
                                required={false}
                                onInputChange={(e: React.ChangeEvent<HTMLInputElement>) => this.handleInputChange(e, this.state.editActivityHubId ?? 0)}
                            />
                        </>
                    }
                </>
            );

            }

        return elements;
 
    }

    private saveTags = () => {
        if (this.state.userTags.some(x => x.name === "" || x.typeID === 0 || x.themeColor === "")) {
            this.props.showAlert("All inputs must be filled", Variant.Danger)
        }
        else
        {
            this.setState({ saving: true })

            api.saveUserTags(this.state.userTags)
                .then(ut => this.saveTagsSuccess(ut))
        }
    }

    private saveTagsSuccess = (status: boolean) => {
        this.setState({ saving: false })

        if (status) {
            this.props.showAlert("Calendar tags successfully saved", Variant.Success)
        }
        else{
            this.props.showAlert("There was an issue with updating calendar tags", Variant.Danger)
        }
    }

    private handleInputChange = (e: React.ChangeEvent<HTMLInputElement>, idx: number) => {
        const { name, value } = e.target;
        const items = [...this.state.userTags]
        items[idx][name] = value;

        this.setState({ userTags: items })
    }

    private enableActivityHubChange = (idx: number, checked: boolean) => {

        const activity : IUserTag = this.state.userTags[idx]

        if (checked || (!checked && activity.targetUnit !== "disable" && this.state.editActivityHubId === undefined)) {
            this.setState({ editActivityHubId: idx})
            this.activityHubTargetByHoursChange(idx, true);
        }
        else{
            this.setState({ editActivityHubId: undefined })
            this.activityHubTargetByHoursChange(idx, false, true);
        }
    }

    private activityHubTargetByHoursChange = (idx: number, checked: boolean, disable?: boolean, hours?: boolean) => {
        const items = [...this.state.userTags]
        let targetUnit = items[idx]["targetUnit"];

        if (disable) {
            targetUnit = "disable";
        }
        else if (checked && (items[idx]["targetUnit"] === "disable" || hours)){
            targetUnit = "hours";
        }
        else if (!checked) {
            targetUnit = "";
        }


        items[idx]["targetUnit"] = targetUnit;
        this.setState({ userTags: items })
    }

    private handleSelectChange = (e: React.ChangeEvent<HTMLSelectElement>, idx: number) => {
        const { name, value } = e.target;
        const items = [...this.state.userTags]
        items[idx][name] = value;
        this.setState({ userTags: items })
    }

    private handleRemoveClick = (idx: number) => {
        const items = [...this.state.userTags]
        items.splice(idx, 1);

        this.setState({ userTags: items })
    }

    private handleAddClick = () => {
        this.setState({ userTags: 
            [...this.state.userTags, {
                    name: "",
                    themeColor: "#000000",
                    targetFrequency: undefined,
                    targetValue: undefined,
                    targetUnit: "disable",
                    startDayOfWeek: DayOfWeek.Monday,
                    endDayOfWeek: DayOfWeek.Sunday,
                    typeID: 0,
                    id: ""
                }
            ]
        })
    }
}