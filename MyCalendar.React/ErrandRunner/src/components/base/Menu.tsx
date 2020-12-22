import * as React from 'react';

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
                <a href="/">Home</a>
                <a href="/neworder">ER New Order</a>
                <a href="/calendar">Calendar</a>
            </div>
        )
    }
}