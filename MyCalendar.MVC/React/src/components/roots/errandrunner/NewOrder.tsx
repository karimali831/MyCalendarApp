import * as React from 'react';

class NewOrder extends React.Component {

    public render() {
        return (
            <div>
                <div className="alert alert-primary" role="alert">
                    This is a primary alert—check it out!
                </div>
                <div className="wrap-login100 p-l-55 p-r-55 p-t-65 p-b-54">
                    <span className="login100-form-title p-b-49">
                        ER New Order
                    </span>
                    <div className="wrap-input100 validate-input m-b-23">
                        <span className="label-input100">Start Date</span>
                        <input type="text" name="" value="test" className="form input100" />
                        <span className="focus-input100" data-symbol="&#xf190;" />
                    </div>
                </div>
            </div>
        );
    }
}

export default NewOrder