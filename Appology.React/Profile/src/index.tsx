import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { Appology } from './components/roots/Appology';
import './index.less';
import registerServiceWorker from './registerServiceWorker';

ReactDOM.render(<Appology />, document.getElementById('root'));

registerServiceWorker();
