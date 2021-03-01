import * as React from 'react';
import { IUser } from 'src/models/IUser';
import { MenuTabs } from 'src/Enums/MenuTabs';
import { Tabs } from '../../utils/Tabs';
import { MenuSection } from 'src/Enums/MenuSection';
import { CalendarGeneral } from './CalendarGeneral';
import { TypeGroup, Variant } from '@appology/react-components';
import { Types } from '../types/Types';
import { CalendarTags } from './CalendarTags';
import IGroup from '@appology/react-components/dist/UserTypes/IGroup';

export interface IOwnProps {
    user: IUser,
    groups: IGroup[],
    showAlert: (txt: string, variant: Variant, timeout?: number) => void
}

export interface IOwnState {
    activeMenuTab: MenuTabs
}

export default class Calendar extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);
        
        this.state = {
            activeMenuTab: MenuTabs.CalendarGeneral
        };
    }

    public render() {
        const activeTab = this.state.activeMenuTab.toString();
        const calendarGroups = this.props.groups.find(x => x.id === TypeGroup.Calendars)
        const tagGroups = this.props.groups.find(x => x.id === TypeGroup.TagGroups)

        return (
            <>
                <Tabs selectedMenu={MenuSection.CalendarSettings} handleSelect={this.menuTabSelected} />
                {
                    activeTab === MenuTabs.CalendarGeneral.toString() ?

                        <CalendarGeneral 
                            showAlert={this.props.showAlert} 
                            userCalendars={this.props.user.userCalendars} 
                            settings={this.props.user.calendarSettings} 
                        />
                    : activeTab === MenuTabs.Calendars.toString() && calendarGroups ? 
                    
                        <Types
                            group={calendarGroups}
                            userId={this.props.user.userInfo.userID}
                            showAlert={this.props.showAlert} 
                            userTypes={this.props.user.userTypes.filter(x => x.groupId === TypeGroup.Calendars)}
                            userBuddys={this.props.user.userBuddys} 
                        />        
                    : activeTab === MenuTabs.CalendarTags.toString() ?

                        <CalendarTags 
                            userTags={this.props.user.userTags}
                            userTagGroups={this.props.user.userTypes.filter(x => x.groupId === TypeGroup.TagGroups)}
                            showAlert={this.props.showAlert}
                        />
                    : activeTab === MenuTabs.CalendarGroups.toString() && tagGroups ?

                        <Types
                            group={tagGroups}
                            userId={this.props.user.userInfo.userID}
                            showAlert={this.props.showAlert} 
                            userTypes={this.props.user.userTypes.filter(x => x.groupId === TypeGroup.TagGroups)}
                            userBuddys={this.props.user.userBuddys} 
                        />    
                    : null
                }

            </>
        );
    }    

    private menuTabSelected = (menuTab: MenuTabs) => {
        this.setState({ activeMenuTab: menuTab })
    }
}