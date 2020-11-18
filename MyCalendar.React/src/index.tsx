import * as React from 'react';
import * as ReactDOM from 'react-dom';
import './index.less';
import registerServiceWorker from './registerServiceWorker';
import { ERNewOrder } from './components/roots/Appology';

// ReactDOM.render(<Appology />, document.getElementById('root'));


ReactDOM.render(<ERNewOrder />, document.getElementById('er-neworder'));

registerServiceWorker();
