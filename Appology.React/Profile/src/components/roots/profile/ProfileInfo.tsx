// import { InputElement, DateElement, TextAreaElement, SelectElement, ToggleSwitch, intIsNullOrEmpty, objIsNullOrEmpty } from '@appology/react-components';
import  { InputElement, Variant } from '@appology/react-components'
import * as React from 'react';
import { api } from 'src/Api/Api';
import { ActionButton } from 'src/components/utils/ActionButtons';
import { IUserInfo } from 'src/models/IUser';

export interface IOwnState {
    userInfo: IUserInfo,
    save: boolean
}

export interface IOwnProps {
    userInfo: IUserInfo,
    showAlert: (txt: string, variant: Variant) => void
}


export class ProfileInfo extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            userInfo: this.props.userInfo,
            save: false
        };
    }

    // public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {

    // }

    public render() {
        return (
            <form onSubmit={this.handleSubmit}>
                <InputElement
                    label="Name"
                    id="name"
                    required={true}
                    defaultValue={this.state.userInfo.name}
                    icon="&#xf207;"
                    onInputChange={this.handleInputChange} 
                />
                <InputElement
                    label="Password"
                    id="password"
                    passwordInput={true}
                    defaultValue={this.state.userInfo.password}
                    icon="&#xf191;"
                    onInputChange={this.handleInputChange} 
                />
                <InputElement
                    label="Email"
                    id="email"
                    required={true}
                    defaultValue={this.state.userInfo.email}
                    icon="&#xf15a;"
                    onInputChange={this.handleInputChange} 
                />
                <InputElement
                    label="Phone Number"
                    id="phoneNumber"
                    defaultValue={this.state.userInfo.phoneNumber}
                    icon="&#xf202;"
                    onInputChange={this.handleInputChange} 
                />
                <ActionButton loading={this.state.save} />
            </form>
        );
    }

    private handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        this.setState({ save: true })
        
        api.saveUserInfo(this.state.userInfo)
            .then(status => this.saveUserInfoSuccess(status));
    }

    private saveUserInfoSuccess = (status: boolean) => {
        this.setState({ save: false })

        if (status) {
            this.props.showAlert("Profile saved successfully", Variant.Success)
        }
        else{
            this.props.showAlert("There was an issue with updating your profile", Variant.Danger)
        }
    }

    private handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {

        this.setState({
            userInfo: { ...this.state.userInfo,    
                [e.target.name]: e.target.value    
            }
        })
    }
}