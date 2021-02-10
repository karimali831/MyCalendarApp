
import IProfilesState, { ProfileState } from './IState';
import { Reducer } from 'redux';
import { ProfileActions, ProfileActionTypes } from './Action';

const ProfileReducer: Reducer<IProfilesState, ProfileActions> =
    (state = ProfileState.intialState, action) => {
        switch (action.type) {
            case ProfileActionTypes.LoadUser:
                return {
                    ...state,
                    ...{ loading: true }
                }

            case ProfileActionTypes.LoadUserSuccessAction:
                return {
                    ...state,
                    ...{ 
                        user: action.user,
                        groups: action.groups,
                        loading: false
                    }
                }

            case ProfileActionTypes.LoadUserFailureAction:
                return {
                    ...state,
                    ...{ loading: false }
                }    

            case ProfileActionTypes.ActiveMenuTabAction:
                return {
                    ...state,
                    ...{ activeMenuTab: action.tab }
                }      


            default:
                return state;
        }
    }

export default ProfileReducer;