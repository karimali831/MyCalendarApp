import { Middleware } from 'redux';
import IStoreState from '../IStoreState';

// this allows class instances to be passed to Redux via dispatch
// it uses the spread operator to create a pure JS object from
// the class instance

export const actionToPlainObject: Middleware<IStoreState, any> =
    store => next => action => {
        if (!action) {
            return;
        }

        return next({ ...action });
    }