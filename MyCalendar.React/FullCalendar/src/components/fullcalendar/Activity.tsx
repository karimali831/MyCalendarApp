import * as React from 'react';
import Alert from 'react-bootstrap/Alert';
import { IActivity } from 'src/models/IActivity';
import { UserAvatar } from './UserAvatar';

interface IOwnProps {
    display: boolean,
    activity: IActivity[],
    onClose: () => void
}

export const UserActivity: React.FC<IOwnProps> = (props) => {
    return (
        props.display ? 
            <Alert key="current-activity" variant="info" onClose={props.onClose} dismissible="dismissible">
                {props.activity.map((ca, idx) => 
                    <div key={idx} style={{padding: 5}}>
                        <UserAvatar avatar={ca.avatar} content={<>{ca.text}</>} />
                    </div>
                )}
            </Alert>
        : null
    )
}