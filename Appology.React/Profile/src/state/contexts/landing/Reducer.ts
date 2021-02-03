
import ILandingsState, { LandingState } from './ILandingState';
import { Reducer } from 'redux';
import { LandingActions, LandingActionTypes } from './Actions';

const LandingReducer: Reducer<ILandingsState, LandingActions> =
    (state = LandingState.intialState, action) => {
        switch (action.type) {
            case LandingActionTypes.SelectedAction:
                return {
                    ...state,
                    ...{
                        filter: ""
                    }
                }


            default:
                return state;
        }
    }

export default LandingReducer;