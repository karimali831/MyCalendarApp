import { CWTLActions, CWTLActionTypes } from "./Actions";
import ICWTLState, { CWTLState } from './ICWTLState';
import { Reducer } from 'redux';

const CWTLReducer: Reducer<ICWTLState, CWTLActions> =
    (state = CWTLState.intialState, action) => {
        switch (action.type) {
            case CWTLActionTypes.CWTL:
                return { ...state, ...{ CWTL: action.CWTLMessage } };

            default:
                return state;
        }
    }

export default CWTLReducer;