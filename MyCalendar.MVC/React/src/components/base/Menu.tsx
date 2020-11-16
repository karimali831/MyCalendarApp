import * as React from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faBars } from '@fortawesome/free-solid-svg-icons'
import { Link } from "react-router-dom";

interface IOwnProps {
}

export interface IOwnState {
    active: string
}

export const AddMenu = (active: string) => 
    <nav className="navbar navbar-default">
        <div className="container-fluid">
            <ul className="nav navbar-nav">
                <li className={active === "spending" ? "active" : undefined}><Link to="/addspending">Add Spending</Link></li>
                <li className={active === "route" ? "active" : undefined} ><Link to="/addroute">Add Route</Link></li>
                <li className={active === "income" ? "active" : undefined}><Link to="/addincome/">Add Income</Link></li>
                <li className={active === "expense" ? "active" : undefined}><Link to="/addexpense/">Add Finance</Link></li>
                <li className={active === "category" ? "active" : undefined}><Link to="/addcategory/">Add Category</Link></li>
                <li className={active === "reminder" ? "active" : undefined}><Link to="/addreminder/">Add Reminder</Link></li>
            </ul>
        </div>
    </nav>

export default class Menu extends React.Component<IOwnProps, IOwnState> {
    constructor(props: IOwnProps) {
        super(props);
        this.state = { 
            active: ""
        };
    }

    public render() {
        return (
            <div className="topnav" id="myTopnav">
                <Link className={"nav-link" + this.state.active} onClick={() => this.onMenuActive()} to="/home">Home</Link>
                <Link className={"nav-link" + this.state.active} onClick={() => this.onMenuActive()} to="/addspending/">Add</Link>
                <Link className={"nav-link" + this.state.active} onClick={() => this.onMenuActive()} to="/finance/">Finances</Link>
                <Link className={"nav-link" + this.state.active} onClick={() => this.onMenuActive()} to="/income/">Incomes</Link>
                <Link className={"nav-link" + this.state.active} onClick={() => this.onMenuActive()} to="/spending/">Spendings</Link>
                <Link className={"nav-link" + this.state.active} onClick={() => this.onMenuActive()} to="/route/">Routes</Link>
                <Link className={"nav-link" + this.state.active} onClick={() => this.onMenuActive()} to="/summary/">Summaries</Link>
                <Link className={"nav-link" + this.state.active} onClick={() => this.onMenuActive()} to="/category/">Categories</Link>
                <Link className={"nav-link" + this.state.active} onClick={() => this.onMenuActive()} to="/reminder/">Reminders</Link>
                <a className="nav-link" href="/finances/settings">Settings</a>
                <a href="javascript:void(0);" className="icon" onClick={() => this.myFunction()}>
                    <FontAwesomeIcon icon={faBars} />
                </a>
            </div>
        )
    }

    private onMenuActive() {
        const x = document.getElementById("myTopnav");

        if (x !== null && x.className === "topnav responsive") {
            x.className = "topnav";
        } 

        this.setState({ ...this.state, 
            ...{ 
                active: " "
            }
        })  
    }

    private myFunction() {
        const x = document.getElementById("myTopnav");

        if (x !== null) {
            x.className += " responsive" 
        }
    }
}