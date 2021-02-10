import { Load, Variant, AlertModal } from '@appology/react-components';
import * as React from 'react';
import Accordion from 'react-bootstrap/Accordion'
import Card from 'react-bootstrap/Card'
import { FaCalendarCheck, FaPenAlt, FaUser, FaUserFriends } from 'react-icons/fa';
import { MenuSection } from 'src/Enums/MenuSection';
import { IGroup } from 'src/models/IGroup';
import { IUser } from 'src/models/IUser';
import { LoadUser } from 'src/state/contexts/profile/Action';
import Calendar from '../calendar/Calendar';
import Profile from '../profile/Profile';
import Write from '../write/Write';

export interface IPropsFromDispatch {
    getUser: () => LoadUser
}

export interface IPropsFromState {
    user?: IUser,
    groups: IGroup[],
    loading: boolean
}

export interface IOwnState {
    activeMenu: MenuSection,
    alertTxt: string,
    alertVariant: Variant,
    alertTimeout?: number
}

type AllProps = IPropsFromState & IPropsFromDispatch;

export default class Landing extends React.Component<AllProps, IOwnState> {

    constructor(props: AllProps) {
        super(props);

        this.state = {
            activeMenu: MenuSection.Profile,
            alertTxt: "",
            alertVariant: Variant.Success,
            alertTimeout: undefined
        };
    }

    public componentDidMount() {
        this.props.getUser();
    }
    
    public render() {
        if (this.props.loading) {
            return <Load withBackground={true} />
        }

        return (
            this.props.user !== undefined ?
                <>
                    {
                        this.state.alertTxt !== "" ?
                            <AlertModal 
                                show={true} 
                                text={this.state.alertTxt} 
                                timeout={this.state.alertTimeout}
                                handleClose={() => this.setState({ alertTxt: "" })} 
                                variant={this.state.alertVariant} />
                        : null
                    }
                    <Accordion defaultActiveKey="0">
                        <Card>
                            <Accordion.Toggle as={Card.Header} eventKey="0">
                                <FaUser /> Profile 
                            </Accordion.Toggle>
                            <Accordion.Collapse eventKey="0">
                                <Card.Body>
                                    <Profile user={this.props.user} showAlert={this.showAlert} />
                                </Card.Body>
                            </Accordion.Collapse>
                        </Card>
                        <Card>
                            <Accordion.Toggle as={Card.Header} eventKey="1">
                                <FaCalendarCheck /> Calendar Settings
                            </Accordion.Toggle>
                            <Accordion.Collapse eventKey="1">
                                <Card.Body>
                                    <Calendar groups={this.props.groups} user={this.props.user} showAlert={this.showAlert} />
                                </Card.Body>
                            </Accordion.Collapse>
                        </Card>
                        <Card>
                            <Accordion.Toggle as={Card.Header} eventKey="2">
                                <FaPenAlt /> Write Settings
                            </Accordion.Toggle>
                            <Accordion.Collapse eventKey="2">
                                <Card.Body>
                                    <Write groups={this.props.groups} user={this.props.user} showAlert={this.showAlert}  />
                                </Card.Body>
                            </Accordion.Collapse>
                        </Card>
                        <Card>
                            <Accordion.Toggle as={Card.Header} eventKey="3">
                                <FaUserFriends /> Buddys
                            </Accordion.Toggle>
                            <Accordion.Collapse eventKey="3">
                                <Card.Body>
                                    Buddys
                                </Card.Body>
                            </Accordion.Collapse>
                        </Card>
                    </Accordion>
                </>
            : null
        )
    }

    private showAlert = (txt: string, variant: Variant, timeout?: number) => {
        this.setState({ 
            alertTxt: txt, 
            alertVariant: variant, 
            alertTimeout: timeout 
        })
    }
}