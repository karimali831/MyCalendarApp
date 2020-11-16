import { createStore, applyMiddleware } from 'redux';
import createSagaMiddleware from 'redux-saga';
import RootReducer from "./RootReducer";
import { rootSaga } from './middleware/sagas/rootSaga';
import { actionToPlainObject } from './middleware/actionToPlainObject';
import { History } from 'history';
import { routerMiddleware } from 'connected-react-router';
import { LoadCustomersAction } from './contexts/landing/Actions';

export default function initialiseStore(history: History<any>) {

	const sagaMiddleware = createSagaMiddleware();

	const composeEnhancers = (window as any).__REDUX_DEVTOOLS_EXTENSION_COMPOSE__;

	const store =
		createStore(
			RootReducer(history),
			composeEnhancers ?
				composeEnhancers(
					applyMiddleware(
						actionToPlainObject,
						sagaMiddleware,
						routerMiddleware(history)
					)
				) :
				applyMiddleware(
					actionToPlainObject,
					sagaMiddleware,
					routerMiddleware(history)
				)
		);

	sagaMiddleware.run(rootSaga);
	store.dispatch(new LoadCustomersAction);

	return store;

};