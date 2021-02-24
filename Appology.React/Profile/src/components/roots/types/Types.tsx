import { InputElement, Load, ToggleSwitch, Variant } from '@appology/react-components';
import * as React from 'react';
import ListGroup from 'react-bootstrap/ListGroup'
import { Tab } from 'react-bootstrap'
import { FaPlus, FaUserFriends } from 'react-icons/fa';
import { EditModal } from 'src/components/utils/EditModal';
import { IUserBuddy } from 'src/models/IUser';
import { api, ITypeChangeResponse } from 'src/Api/Api';
import { IUserType, IUserTypeDTO } from 'src/models/IUserType';
import { IGroup } from 'src/models/IGroup';
import { TypesTree } from './TypesTree';

export interface IOwnState {
    userTypes: IUserType[],
    selectedType: IUserTypeDTO,
    save: boolean,
    delete: boolean,
    move: boolean
}

export interface IOwnProps {
    userTypes: IUserType[],
    userBuddys: IUserBuddy[],
    userId: string,
    group: IGroup,
    showAlert: (txt: string, variant: Variant, timeout?: number) => void
}


export class Types extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            userTypes: this.props.userTypes,
            selectedType: this.initialStateSelectedType(),
            save: false,
            delete: false,
            move: false
        };
    }

    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (JSON.stringify(this.props.userTypes) !== JSON.stringify(prevProps.userTypes)) {
            this.setState({ userTypes: this.props.userTypes })
        }
    }

    public render() {

        const adding = this.state.selectedType.id === 0;

        return (
            <Tab.Container id={`user-types-${this.props.group}`}>
                <ListGroup>
                    {this.state.move && <Load withBackground={true} />}
                    <ListGroup.Item 
                        style={{ 
                            backgroundColor: adding ? "#007bff" : "rgb(255, 255, 255)",
                            color: adding ? "#fff" : "rgb(0, 0, 0)",
                            borderColor: adding ? "#007bff" : "rgba(0,0,0,.125)"
                        }}  
                        eventKey={0} 
                        onClick={() => this.selectedType()}
                        >
                        <FaPlus /> Add
                    </ListGroup.Item>
                    <ListGroup.Item>
                        <TypesTree 
                            userTypes={this.state.userTypes}
                            group={this.props.group}
                            selectedTypeId={this.state.selectedType.id}
                            moveType={this.moveType}
                            selectedType={this.selectedType}
                        />
                    </ListGroup.Item>
                </ListGroup>
        
                <Tab.Content>
                    {
                        this.state.selectedType.id !== undefined && 
                        <Tab.Pane eventKey={this.state.selectedType.id}>
                            <EditModal 
                                title=""
                                save={this.state.save}
                                delete={this.state.selectedType.id === 0 ? undefined : this.state.delete}
                                content={this.form()}
                                show={true}
                                handleClose={() => this.handleClose()}
                                onDelete={() => this.delete()}
                                onSave={() => this.save()} />
                        </Tab.Pane>
                    }
                </Tab.Content>
            </Tab.Container>
        );
    }

    private moveType = (id: number, dropPos: number, moveToNode: IUserType) => {
        this.setState({ move: true })

        api.moveUserType(id, this.props.group.id, dropPos === -1 ? undefined : moveToNode.key)
            .then(response => this.typeChangeSuccess(response))
    }

    private typeChangeSuccess = (response: ITypeChangeResponse) => {
        this.props.showAlert(response.responseMsg, response.status ? Variant.Success : Variant.Danger)

        this.setState({ 
            userTypes: response.userTypes,
            delete: false,
            save: false,
            move: false,
            selectedType: this.initialStateSelectedType() 
        })
    }

    private handleClose = () => {
        this.setState({ selectedType: this.initialStateSelectedType() })
    }

    private selectedType = (type?: IUserType, addToKey?: number) => {
        let selectedType : IUserTypeDTO;

        if (type !== undefined) {
            selectedType = {
                id: type.key,
                name: type.title,
                groupId: type.groupId,
                inviteeIds: type.inviteeIdsList.join(','),
                superTypeId: type.superTypeId
            }
        }
        else{
            selectedType = this.initialStateSelectedType(0, addToKey)
        }

        this.setState({ selectedType: selectedType })
    }

    private initialStateSelectedType = (addKey?: number, addToKey?: number): IUserTypeDTO => {
        const initialSelectedType: IUserTypeDTO = {
            id: addKey,
            name: "",
            inviteeIds: "",
            superTypeId: addToKey,
            groupId: this.props.group.id
        }

        return initialSelectedType;
    }

    private delete = () => {
        if (this.state.selectedType.id !== undefined) {
            this.setState({ delete: true })

            api.deleteUserType(this.state.selectedType.id, this.props.group.id)
                .then(r => this.typeChangeSuccess(r))
        }
    }

    private save = () => {
        if (this.state.selectedType.name === "") {
            this.props.showAlert("You must enter a name", Variant.Danger, 2000)
            return;
        }

        this.setState({ save: true })

        api.saveUserType(this.state.selectedType)
            .then(response => this.typeChangeSuccess(response))
    }

    private form = (): JSX.Element[] => {
        const type = this.state.selectedType.id !== undefined ? this.state.selectedType : undefined;

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
                <br />
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
                        onChange={(checked: boolean) => this.buddyChange(checked, u.userID)} />
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

    private buddyChange = (checked: boolean, userID: string) => {
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