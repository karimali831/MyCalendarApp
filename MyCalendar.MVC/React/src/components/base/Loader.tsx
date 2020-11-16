import * as React from 'react';
import './Loader.css'
import "react-loader-spinner/dist/loader/css/react-spinner-loader.css"
import Loader from 'react-loader-spinner';

interface IOwnProps {
    text?: string | null
    type?: string | null
}

export const Load: React.FC<IOwnProps> = (props) => {
    return (
        <div className={props.type !== null ? props.type : "loader"}>
            <Loader type="Puff" color="#000" height={50} width={50} timeout={20000} />
            {
                props.text && props.text !== null ? 
                    <span className="text"><strong>{props.text}</strong></span> 
                : 
                    ""
            }
        </div>
    )
}