import * as React from 'react';
import { Link } from "react-router-dom";

interface IOwnProps {
}

export interface IOwnState {
    active: string
}


export default class Menu extends React.Component<IOwnProps, IOwnState> {
    constructor(props: IOwnProps) {
        super(props);
        this.state = { 
            active: ""
        };
    }

    public render() {
        return (
            <div>
                <Link to="/home">Home</Link>
            </div>
        )
    }
}