import * as React from 'react';

interface IOwnProps {

}

export const Load: React.FC<IOwnProps> = (props) => {
    return (
        <div className="loader" style={{ display: "block" }} />
    )
}