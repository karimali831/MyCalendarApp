import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { Appology } from './components/roots/Appology';
import './index.less';
import registerServiceWorker from './registerServiceWorker';
import LogRocket from 'logrocket';
LogRocket.init('uzvvjb/appology');

ReactDOM.render(<Appology />, document.getElementById('root'));

registerServiceWorker();
