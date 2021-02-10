import * as React from 'react';
import Form from 'react-bootstrap/Form'
import Table from 'react-bootstrap/Table'
import { IUserTag } from 'src/models/IUserTag';
import Button from 'react-bootstrap/Button'
import InputGroup from 'react-bootstrap/InputGroup'
import { FaMinus, FaPlus } from 'react-icons/fa';
import { SaveButton } from 'src/components/utils/ActionButtons';
import { api } from 'src/Api/Api';
import { Variant } from '@appology/react-components';
import { IUserType } from 'src/models/IUserType';

export interface IOwnState {
    userTags: IUserTag[],
    userTagGroups: IUserType[],
    saving: boolean
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
            saving: false
        };
    }
    public render() {
        return (
            <Table responsive={true}>
                <thead>
                    <tr>
                        <th>Label</th>
                        <th>Theme</th>
                        <th>Group</th>
                    </tr>
                </thead>
                <tbody>
                {
                    this.state.userTags.map((x, i) => {
                        return (
                            <>
                                <tr>
                                    <td>
                                        <Form.Control 
                                            htmlSize={2}
                                            required={true}
                                            name="name"
                                            value={x.name}
                                            onChange={(e: React.ChangeEvent<HTMLInputElement>) => this.handleInputChange(e, i)} />
                                    </td>
                                    <td>
                                        <Form.Control 
                                        htmlSize={1}
                                            required={true}
                                            name="themeColor"
                                            type="color"
                                            value={x.themeColor}
                                            onChange={(e: React.ChangeEvent<HTMLInputElement>) => this.handleInputChange(e, i)} />
                                    </td>
                                    <td>
                                        <InputGroup className="mb-2 mr-sm-2">
                                            <Form.Control 
                                                as="select" 
                                                required={true}
                                                name="typeID"
                                                value={x.typeID}
                                                onChange={(e: React.ChangeEvent<HTMLSelectElement>) => this.handleGroupChange(e, i)}>
                                                    {this.state.userTagGroups.map(g => 
                                                        <option key={g.id} value={g.id}>{g.name}</option>
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
                                                <SaveButton saving={this.state.saving} onSaveClick={() => this.saveTags()} />
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
        );
    }

    private saveTags = () => {
        this.setState({ saving: true })

        api.saveUserTags(this.state.userTags)
            .then(ut => this.saveTagsSuccess(ut))
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

    private handleGroupChange = (e: React.ChangeEvent<HTMLSelectElement>, idx: number) => {
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
                    themeColor: "",
                    typeID: 0,
                    id: ""
                }
            ]
        })
    }
}