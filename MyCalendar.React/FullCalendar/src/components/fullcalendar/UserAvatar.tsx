import * as React from 'react';
import { rootUrl } from '../utils/Utils';

interface IOwnProps {
    avatar: string,
    content?: JSX.Element
}

export const UserAvatar: React.FC<IOwnProps> = (props) => {
    return (
        props.avatar.length === 2 ?
            <p default-avatar={props.avatar}> {props.content}</p> :
            <><img width={30} height={30} src={rootUrl + props.avatar} /> {props.content}</>  
    )
}