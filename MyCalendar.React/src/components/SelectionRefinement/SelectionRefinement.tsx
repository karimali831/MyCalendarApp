import * as React from 'react'
import { IBaseModel } from 'src/models/IBaseModel';
import { RefinementInput } from './RefinementInput';

interface IOwnState {
    loading: boolean
    showResults: boolean
}

interface IOwnProps {
    filter?: string | null
    onChange: (filter: string) => void
}

export class SelectionRefinement<T extends IBaseModel<T>> extends React.Component<IOwnProps, IOwnState> {

    private inputRef = React.createRef<RefinementInput>();

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            loading: false,
            showResults: false
        };
    }

    public render() {
        return (

            <div className="wrap-input100 validate-input m-b-23">
                <span className="label-input100">Customer Search</span>
                <RefinementInput ref={this.inputRef} filter={this.props.filter} placeholder="Search by customer info..." onChange={this.props.onChange} />
                <span className="focus-input100" data-symbol="&#xf190;" />
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