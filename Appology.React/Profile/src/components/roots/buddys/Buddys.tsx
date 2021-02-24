import { Load, UserAvatar, Variant } from '@appology/react-components';
import * as React from 'react';
import ListGroup from 'react-bootstrap/ListGroup'
import { FaShare, FaTimes } from 'react-icons/fa';
import { api, IRemoveBuddyResponse } from 'src/Api/Api';
import { rootUrl } from 'src/components/utils/Utils';
import { IUserBuddy } from 'src/models/IUser';
import { BuddyInvite } from './BuddyInvite';

export interface IOwnState {
    buddys: IUserBuddy[],
    removeeId?: string
}

export interface IOwnProps {
    buddys: IUserBuddy[],
    confirmAction?: boolean,
    inviterShareLink: string,
    showAlert: (txt: string, variant: Variant, timeout?: number) => void,
    confirmationHandled: () => void,
    showConfirmation: (bodyContent: JSX.Element, variant: Variant) => void
}


export class Buddys extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            buddys: this.props.buddys,
            removeeId: undefined
        };
    }

    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (prevProps.confirmAction !== this.props.confirmAction && this.state.removeeId !== undefined) {
            if (this.props.confirmAction) {
                this.removeBuddy(this.state.removeeId, true)
            }
            else{
                this.setState({ removeeId: undefined })
                this.props.confirmationHandled();
            }
        }
    }

    public render() {

        return (
            <>
                <ListGroup>
                    <ListGroup.Item key={0}>
                        <h6><FaShare /> Share this link with a user to add each other as a buddy</h6>
                        <BuddyInvite inviterShareLink={this.props.inviterShareLink} showAlert={this.props.showAlert} />
                    </ListGroup.Item>
                </ListGroup>
                <ListGroup horizontal={true}>
                    {
                        this.state.buddys.map(b => 
                            <ListGroup.Item key={b.userID} style={{ width: 130, paddingTop: 10, paddingLeft: 5 }}>
                                <UserAvatar rootUrl={rootUrl} width={30} height={30} avatar={b.avatar} content={<strong>{b.name}</strong>} />
                                <div className="buddy-remove">
                                    {
                                        this.state.removeeId === b.userID ? 
                                            <Load smallSize={true} /> : 
                                            <FaTimes onClick={() => this.removeBuddy(b.userID, false)} />
                                    }
                                </div>
                            </ListGroup.Item>
                        )
                    }
                </ListGroup>
            </>
        );
    }

    private removeBuddy = (buddyId: string, existConfirm: boolean) => {
        this.setState({ removeeId: buddyId})

        api.removeBuddy(buddyId, existConfirm)
            .then(response => this.removeBuddySuccess(response));
    }

    private removeBuddySuccess = (response: IRemoveBuddyResponse) => {
        if (response.responseVariant !== Variant.Success){
            this.props.showConfirmation(<>{response.responseMsg}</>, response.responseVariant)
        }
        else
        {
            this.setState(prevState => ({ 
                buddys: prevState.buddys.filter(x => x.userID !== this.state.removeeId),
                removeeId: undefined
            }));

            this.props.confirmationHandled();
            this.props.showAlert(response.responseMsg, response.responseVariant)
        }
    }
}