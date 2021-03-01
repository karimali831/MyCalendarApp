import * as React from 'react';
import { IUser } from 'src/models/IUser';
import { MenuTabs } from 'src/Enums/MenuTabs';
import { Tabs } from '../../utils/Tabs';
import { MenuSection } from 'src/Enums/MenuSection';
import { TypeGroup, Variant } from '@appology/react-components';
import { Types } from '../types/Types';
import IGroup from '@appology/react-components/dist/UserTypes/IGroup';

export interface IOwnProps {
    user: IUser,
    groups: IGroup[],
    showAlert: (txt: string, variant: Variant, timeout?: number) => void
}

export interface IOwnState {
    activeMenuTab: MenuTabs
}

export default class Write extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);
        
        this.state = {
            activeMenuTab: MenuTabs.WriteFolders
        };
    }

    public render() {
        const activeTab = this.state.activeMenuTab.toString();
        const folderGroups = this.props.groups.find(x => x.id === TypeGroup.DocumentFolders)

        return (
            <>
                <Tabs selectedMenu={MenuSection.WriteSettings} handleSelect={this.menuTabSelected} />
                {
                    activeTab === MenuTabs.WriteFolders.toString() && folderGroups ?

                    <Types
                        group={folderGroups}
                        userId={this.props.user.userInfo.userID}
                        showAlert={this.props.showAlert} 
                        userTypes={this.props.user.userTypes.filter(x => x.groupId === TypeGroup.DocumentFolders)}
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