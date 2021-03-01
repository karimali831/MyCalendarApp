import * as React from 'react';
import Nav from 'react-bootstrap/Nav'
import { MenuSection } from 'src/Enums/MenuSection';
import { MenuTabs } from 'src/Enums/MenuTabs';

export interface IOwnProps {
    selectedMenu: MenuSection,
    handleSelect: (menuTab: MenuTabs) => void
}

const defaultActiveKey = (menu: MenuSection) : MenuTabs => {
    switch(menu) {
        case MenuSection.Profile:
            return MenuTabs.ProfileInfo;

        case MenuSection.CalendarSettings:
            return MenuTabs.CalendarGeneral;

        case MenuSection.WriteSettings:
            return MenuTabs.WriteFolders;

        default:
            return MenuTabs.ProfileInfo;
      }
}

export const Tabs: React.FC<IOwnProps> = (props) => {
    return (
        <>
            <Nav variant="tabs" defaultActiveKey={defaultActiveKey(props.selectedMenu)} onSelect={props.handleSelect}>
                {
                    props.selectedMenu === MenuSection.Profile ?
                        <>
                            <Nav.Item>
                                <Nav.Link eventKey={MenuTabs.ProfileInfo}>Info</Nav.Link>
                            </Nav.Item>
                            <Nav.Item>
                                <Nav.Link eventKey={MenuTabs.ProfileAvatar}>Avatar</Nav.Link>
                            </Nav.Item>
                        </>
                    : props.selectedMenu === MenuSection.CalendarSettings ?
                        <>
                            <Nav.Item>
                                <Nav.Link eventKey={MenuTabs.CalendarGeneral}>General</Nav.Link>
                            </Nav.Item>
                            <Nav.Item>
                                <Nav.Link eventKey={MenuTabs.Calendars}>Calendars</Nav.Link>
                            </Nav.Item>
                            <Nav.Item>
                                <Nav.Link eventKey={MenuTabs.CalendarTags}>Tags</Nav.Link>
                            </Nav.Item>
                            <Nav.Item>
                                <Nav.Link eventKey={MenuTabs.CalendarGroups}>Tag Groups</Nav.Link>
                            </Nav.Item>
                        </>
                    : props.selectedMenu === MenuSection.WriteSettings ?
                        <>
                            <Nav.Item>
                                <Nav.Link eventKey={MenuTabs.WriteFolders}>Folders</Nav.Link>
                            </Nav.Item>
                        </>
                    : null
                }
    
            </Nav>
            <br />
        </>
    )
}