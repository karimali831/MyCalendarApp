import * as React from 'react'
import { IBaseModel } from 'src/models/IBaseModel';
import { RefinementInput } from './RefinementInput';


interface IOwnProps {
    filter: string 
    onChange: (filter: string) => void
    onLoading: (loading: boolean) => void
}

export class SelectionRefinement<T extends IBaseModel<T>> extends React.Component<IOwnProps> {

    private inputRef = React.createRef<RefinementInput>();

    public render() {
        return (

            <div className="wrap-input100 validate-input m-b-23">
                <span className="label-input100">Customer Search</span>
                <RefinementInput ref={this.inputRef} filter={this.props.filter} placeholder="Search by customer info..." onChange={this.props.onChange} onLoading={this.props.onLoading} />
                <span className="focus-input100" data-symbol="&#xf1c3;" />
            </div>

        );
    }

    public componentDidMount = () => {
        this.focusInput()
    }

    public focusInput = () => {
        // if (this.inputRef.current) {
        //     this.inputRef.current.focus();
        // }
    }
}