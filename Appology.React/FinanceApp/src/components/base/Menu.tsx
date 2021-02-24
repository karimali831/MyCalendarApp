import * as React from 'react';
import { Link } from "react-router-dom";
import { appPathUrl } from '../utils/Utils';
import { push } from "connected-react-router";
import { connect } from 'react-redux';
import Nav from 'react-bootstrap/Nav'
import NavDropdown from 'react-bootstrap/NavDropdown'
import Navbar from 'react-bootstrap/Navbar';

export interface IPropsFromState {
    location: string,
    
}

export const Menu: React.FC<IPropsFromState> = (props) => {
    return (
        <Navbar id="menu" collapseOnSelect={true} expand="lg">
            <Navbar.Toggle aria-controls="basic-navbar-nav" />
            <Navbar.Collapse id="responsive-navbar-nav">
                <Nav className="mr-auto">
                    <Nav.Item>
                        <Nav.Link eventKey="/" active={!props.location || props.location === "/" || props.location === "/home"} as={Link} to={`${appPathUrl}/home`}>Home</Nav.Link>
                    </Nav.Item>
                    <NavDropdown title="Add" id="nav-dropdown" active={props.location === "addspending" || props.location === "addincome" || props.location === "addexpense" || props.location === "addcategory" || props.location === "addreminder"}>
                        <NavDropdown.Item eventKey="addspending" active={props.location === "addspending"} as={Link} to={`${appPathUrl}/addspending`}>Spending</NavDropdown.Item>
                        <NavDropdown.Item eventKey="addincome" active={props.location === "addincome"} as={Link} to={`${appPathUrl}/addincome`}>Income</NavDropdown.Item>
                        <NavDropdown.Item eventKey="addfinance" active={props.location === "addexpense"} as={Link} to={`${appPathUrl}/addexpense`}>Finance</NavDropdown.Item>
                        <NavDropdown.Item eventKey="addcategory" active={props.location === "addcategory"} as={Link} to={`${appPathUrl}/addcategory`}>Category</NavDropdown.Item>
                        <NavDropdown.Item eventKey="addreminder" active={props.location === "addreminder"} as={Link} to={`${appPathUrl}/addreminder`}>Reminder</NavDropdown.Item>
                    </NavDropdown>
                    <Nav.Item>
                        <Nav.Link eventKey="finance" active={props.location === "finance"} as={Link} to={`${appPathUrl}/finance`}>Finances</Nav.Link>
                    </Nav.Item>
                    <Nav.Item>
                        <Nav.Link eventKey="income" active={props.location === "income"}as={Link} to={`${appPathUrl}/income`}>Incomes</Nav.Link>
                    </Nav.Item>
                    <Nav.Item>
                        <Nav.Link eventKey="spending" active={props.location === "spending"} as={Link} to={`${appPathUrl}/spending`}>Spendings</Nav.Link>
                    </Nav.Item>
                    <Nav.Item>
                        <Nav.Link eventKey="category" active={props.location === "category"} as={Link} to={`${appPathUrl}/category`}>Categories</Nav.Link>
                    </Nav.Item>
                    <Nav.Item>
                        <Nav.Link eventKey="reminder" active={props.location === "reminder"} as={Link} to={`${appPathUrl}/reminder`}>Reminders</Nav.Link>
                    </Nav.Item>
                </Nav>
            </Navbar.Collapse>
        </Navbar>
    )
}

export default connect(
    null,
    { push }
  )(Menu);