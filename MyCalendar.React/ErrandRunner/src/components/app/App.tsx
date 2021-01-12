import * as React from 'react';
import './App.css';
import { Route, Switch } from 'react-router-dom';
import { Provider } from 'react-redux';
import initialiseStore from '../../state/InitialiseStore';
import { ConnectedRouter } from 'connected-react-router';
import { createBrowserHistory } from 'history';
import { NewOrder } from '../roots/order/NewOrder';

class App extends React.Component {
  private appElement: React.RefObject<HTMLDivElement> = React.createRef<HTMLDivElement>();

  public render() {
    const store = initialiseStore(createBrowserHistory());

    return (
      <Provider store={store}>
        <ConnectedRouter history={(createBrowserHistory())}>
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