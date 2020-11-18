﻿import * as React from 'react';
import './App.css';
import { Route, Switch } from 'react-router-dom';
import { Provider } from 'react-redux';
import initialiseStore from '../../state/InitialiseStore';
import { ConnectedRouter } from 'connected-react-router';
import { createHashHistory, createBrowserHistory } from 'history';
import NewOrder from '../roots/errandrunner/NewOrder';
import Menu from '../base/Menu';

const browserHistory = history.pushState ? createBrowserHistory() : createHashHistory();

class App extends React.Component {
  private appElement: React.RefObject<HTMLDivElement> = React.createRef<HTMLDivElement>();

  public render() {
    const store = initialiseStore(browserHistory);

    return (
      <Provider store={store}>
        <ConnectedRouter history={(browserHistory)}>
          <Menu />
            <div className="App" ref={this.appElement}>
              <Switch>
                  <Route exact={true} path="/" component={NewOrder} />
                  <Route exact={true} path="" component={NewOrder} />
                  <Route path="/home" component={NewOrder} />
              </Switch>
            </div>
        </ConnectedRouter>
      </Provider>
    );
  }
}
  
export default App;