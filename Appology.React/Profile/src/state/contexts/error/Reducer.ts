import { ErrorActions, ErrorActionTypes } from "./Actions";
import IErrorState, { ErrorState } from './IErrorState';
import { Reducer } from 'redux';

const ErrorReducer: Reducer<IErrorState, ErrorActions> =
    (state = ErrorState.intialState, action) => {
        switch (action.type) {
            case ErrorActionTypes.ReportError:
                return { ...state, ...{ error: action.errorMessage } };

            case ErrorActionTypes.HideError:
                return { ...state, ...{ error: null } }

            default:
                return state;
        }
    }

export default ErrorReducer;