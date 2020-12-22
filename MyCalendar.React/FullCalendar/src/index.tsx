import * as React from 'react';
import * as ReactDOM from 'react-dom';
import Calendar from './components/fullcalendar/Calendar';
import registerServiceWorker from './registerServiceWorker';

ReactDOM.render(<Calendar />, document.getElementById('root'));

registerServiceWorker();
