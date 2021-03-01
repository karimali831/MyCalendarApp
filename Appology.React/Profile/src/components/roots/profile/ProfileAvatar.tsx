import * as React from 'react';
// import Avatar from 'avataaars'
import  { UserAvatar }  from '@appology/react-components'
import { rootUrl } from 'src/components/utils/Utils';

export interface IOwnState {
}

export interface IOwnProps {
    avatar: string
}


export class ProfileAvatar extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
        };
    }

    public render() {
        return (
            <UserAvatar rootUrl={rootUrl} size="large" avatar={this.props.avatar} />
            // <Avatar
            //     avatarStyle='Circle'
            //     topType='Hijab'
            //     accessoriesType='Wayfarers'
            //     // hatColor='PastelGreen'
            //     facialHairType='Blank'
            //     clotheType='Hoodie'
            //     clotheColor='PastelRed'
            //     eyeType='Squint'
            //     eyebrowType='UpDownNatural'
            //     mouthType='Twinkle'
            //     skinColor='Yellow'
            //     />
                
        );
    }
}