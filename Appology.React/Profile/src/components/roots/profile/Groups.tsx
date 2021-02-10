import { InputElement, ToggleSwitch, Variant } from '@appology/react-components';
import * as React from 'react';
import ListGroup from 'react-bootstrap/ListGroup'
import { Tab, Row, Col } from 'react-bootstrap'
import { FaAngleRight, FaAngleDown, FaPlus, FaUserFriends } from 'react-icons/fa';
import { EditModal } from 'src/components/utils/EditModal';
import { IUserBuddy } from 'src/models/IUser';
import { api, IDeleteTypeResponse, ISaveTypeResponse } from 'src/Api/Api';
import { IUserType, IUserTypeDTO } from 'src/models/IUserType';
import { IGroup } from 'src/models/IGroup';

export interface IOwnState {
    userTypes: IUserType[],
    selectedType: IUserTypeDTO,
    save: boolean,
    delete: boolean
}

export interface IOwnProps {
    userTypes: IUserType[],
    userBuddys: IUserBuddy[],
    userId: string,
    group: IGroup,
    showAlert: (txt: string, variant: Variant, timeout?: number) => void
}


export class Groups extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            userTypes: this.props.userTypes,
            selectedType: this.initialStateSelectedType(),
            save: false,
            delete: false
        };
    }

    public render() {
        return (
            <Tab.Container id="user-types">
                <Row>
                    <Col sm={4}>
                        <ListGroup>
                            <ListGroup.Item action={true} eventKey="add" onClick={() => this.selectedType()}>
                                <FaPlus /> Add
                            </ListGroup.Item>
                        {
                            this.state.userTypes && this.state.userTypes.length > 0 ?
                                this.state.userTypes.map(uc => 
                                    <ListGroup.Item key={uc.id} action={true} active={this.state.selectedType.id === uc.id} eventKey={uc.id} onClick={() => this.selectedType(uc)}>
                                        {this.state.selectedType.id === uc.id ? <FaAngleDown /> : <FaAngleRight />} {uc.name}
                                    </ListGroup.Item>
                                )
                            : null
                        }
                        </ListGroup>
                    </Col>
                    <Col sm={8}>
                        <Tab.Content>
                            {
                                <Tab.Pane eventKey="add">
                                    <EditModal 
                                        title="" 
                                        save={this.state.save}
                                        content={this.form()}
                                        onSave={() => this.save()} />
                                </Tab.Pane>
                            }
                            {
                                this.state.userTypes && this.state.userTypes.length > 0 ?
                                    this.state.userTypes.map(uc => {
                                            return (
                                                <Tab.Pane key={uc.id} eventKey={uc.id}>
                                                    <EditModal 
                                                        title="" 
                                                        save={this.state.save}
                                                        delete={this.state.delete}
                                                        content={this.form(uc)} 
                                                        onDelete={() => this.delete()}
                                                        onSave={() => this.save()} />
                                                </Tab.Pane>

                                            )
                                        }
                                    )
                                : null
                            }
        
                        </Tab.Content>
                    </Col>
                </Row>
            </Tab.Container>
        );
    }

    private selectedType = (type?: IUserType) => {
        let selectedType : IUserTypeDTO;

        if (type !== undefined) {
            selectedType = {
                id: type.id,
                name: type.name,
                groupId: type.groupId,
                inviteeIds: type.inviteeIdsList.join(',')
            }
        }
        else{
            selectedType = this.initialStateSelectedType()
        }

        this.setState({ selectedType: selectedType })
    }

    private initialStateSelectedType = (): IUserTypeDTO => {
        const initialSelectedType: IUserTypeDTO = {
            id: undefined,
            name: "",
            inviteeIds: "",
            groupId: this.props.group.id
        }

        return initialSelectedType;
    }

    private delete = () => {
        if (this.state.selectedType.id !== undefined) {
            this.setState({ delete: true })

            api.deleteUserType(this.state.selectedType.id)
                .then(r => this.deleteSuccess(r))
        }
    }

    private deleteSuccess = (deleteResponse: IDeleteTypeResponse) => {
        this.setState({ delete: false })
     
        if (deleteResponse.status) {

            this.setState({ userTypes: this.state.userTypes.filter(item => item.id !== this.state.selectedType.id) })
            this.props.showAlert("Successfully removed", Variant.Success)
        }
        else{
            this.props.showAlert(deleteResponse.message, Variant.Danger)
        }
    }

    private save = () => {
        if (this.state.selectedType.name === "") {
            this.props.showAlert("You must enter a name", Variant.Danger, 2000)
            return;
        }

        this.setState({ save: true })

        api.saveUserType(this.state.selectedType)
            .then(response => this.saveSuccess(response))
    }

    private saveSuccess = (response: ISaveTypeResponse) => {
        this.setState({ save: false })

        if (response.type !== undefined) {

            if (this.state.selectedType.id) {
                const selectedTypeIdx = this.state.userTypes.map(o => o.id).indexOf(this.state.selectedType.id);

                this.setState(prevState => ({
                    userTypes: [
                        ...prevState.userTypes.slice(0, selectedTypeIdx),
                        {
                            ...prevState.userTypes[selectedTypeIdx],
                            name: this.state.selectedType.name,
                            inviteeIdsList: this.state.selectedType.inviteeIds.split(",")
                        },
                        ...prevState.userTypes.slice(selectedTypeIdx + 1)
                    ]
                }));
            }
            else{
                this.setState({ userTypes: [...this.state.userTypes, response.type]}) 
            }

            this.props.showAlert("Saved successfully", Variant.Success)
        }
        else{
            this.props.showAlert("There was an issue updating", Variant.Danger)
        }
    }

    private form = (type?: IUserType): JSX.Element[] => {
        const elements: JSX.Element[] = [
            <>
                <InputElement
                    key={type?.id}
                    label="Name"
                    icon="&#xf32f;"
                    id="name"
                    defaultValue={type?.name}
                    required={true}
                    onInputChange={this.inputChange}
                />
                <FaUserFriends /> <span className="label-input100">{this.props.group.inviteDescription}</span>
                <hr />
            </>
        ]

        if (this.props.userBuddys && this.props.userBuddys.length > 0) {
            this.props.userBuddys.map(u => {
                elements.push(
                    <ToggleSwitch 
                        key={`${type?.id}-${u.userID}`} 
                        id={`${type?.id}-${u.userID}`} 
                        name={u.name} 
                        checked={this.state.selectedType.inviteeIds.split(",").includes(u.userID)} 
                        onChange={(checked: boolean) => this.buddyChange(checked, u.userID, type)} />
                )
            })
        }
        else{
            elements.push(
                <div>No buddys</div>
            )
        } 

        return elements;
    }

    private inputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.setState({
            selectedType: { ...this.state.selectedType,    
                name: e.target.value
            }
        })
    }

    private buddyChange = (checked: boolean, userID: string, type?: IUserType) => {
        let inviteeIds : string[] = []
  
        if (this.state.selectedType.inviteeIds.length > 0) {
            const invitees = this.state.selectedType.inviteeIds.split(",");

            if (checked) {
                inviteeIds = [...invitees, userID]
            }
            else{
                inviteeIds = invitees.filter(item => item !== userID)
            }
        }
        else{
            inviteeIds.push(userID)
        }

        this.setState({
 
            selectedType: { ...this.state.selectedType,    
                inviteeIds: inviteeIds.join(',')
            }
        })
    }
}