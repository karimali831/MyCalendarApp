import * as React from 'react';
import Alert from 'react-bootstrap/Alert';
import { FaUserClock } from 'react-icons/fa';

interface IOwnProps {
    display: boolean,
    activity: string[]
    onClose: () => void
}

export const UserActivity: React.FC<IOwnProps> = (props) => {
    return (
        props.display ? 
            <Alert key="current-activity" variant="info" onClose={props.onClose} dismissible="dismissible">
                {props.activity.map((ca, idx) => 
                    <div key={idx}><FaUserClock /> {ca}</div>
                )}
            </Alert>
        : null
    )
}