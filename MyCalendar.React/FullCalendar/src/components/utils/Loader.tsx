import * as React from 'react';

interface IOwnProps {
    display: boolean
}

export const EventLoader: React.FC<IOwnProps> = (props) => {
    return (
        <div className="loader loader-large" id="event-load" style={{display: props.display ? "block" : "none"}}>
            <div />
            <div />
            <div />
            <div />
            <div />
        </div> 
    )
}