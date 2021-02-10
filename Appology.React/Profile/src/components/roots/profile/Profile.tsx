import * as React from 'react';
import { IUser } from 'src/models/IUser';
import { ProfileInfo } from './ProfileInfo';
import { MenuTabs } from 'src/Enums/MenuTabs';
import { Tabs } from '../../utils/Tabs';
import { MenuSection } from 'src/Enums/MenuSection';
import { Variant } from '@appology/react-components';
import { ProfileAvatar } from './ProfileAvatar';

export interface IOwnProps {
    user: IUser,
    showAlert: (txt: string, variant: Variant) => void
}

export interface IOwnState {
    activeMenuTab: MenuTabs
}

export default class Profile extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);
        
        this.state = {
            activeMenuTab: MenuTabs.ProfileInfo
        };
    }

    public render() {
        const activeTab = this.state.activeMenuTab.toString();
        return (
            <>
                <Tabs selectedMenu={MenuSection.Profile} handleSelect={this.menuTabSelected} />
                {
                    activeTab === MenuTabs.ProfileInfo.toString() ?
                        <ProfileInfo userInfo={this.props.user.userInfo} showAlert={(txt, variant) => this.props.showAlert(txt, variant)} />
                    : activeTab === MenuTabs.ProfileAvatar.toString() ?
                        <ProfileAvatar avatar={this.props.user.avatar} />
                    : null
                }

            </>
        );
    }    

    private menuTabSelected = (menuTab: MenuTabs) => {
        this.setState({ activeMenuTab: menuTab })
    }
}