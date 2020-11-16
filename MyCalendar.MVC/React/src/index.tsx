import * as React from 'react';
import * as ReactDOM from 'react-dom';
import './index.less';
import registerServiceWorker from './registerServiceWorker';
import { MyFinanceApp } from './components/roots/MyFinanceApp';

ReactDOM.render(
  <MyFinanceApp />,
  document.getElementById('root') || document.createElement('div') // for testing purposes
  
);

registerServiceWorker();
