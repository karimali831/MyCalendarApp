import * as React from 'react'
import { IBaseModel } from 'src/models/IBaseModel';
import { RefinementInput } from './RefinementInput';

interface IOwnState {
    filter: string,
    loading: boolean
    showResults: boolean
}

interface IOwnProps {
    filter?: string,
    filterChanged: (filter: string) => void
}

export class SelectionRefinement<T extends IBaseModel<T>> extends React.Component<IOwnProps, IOwnState> {

    private inputRef = React.createRef<RefinementInput>();

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            filter: this.props.filter !== undefined ? this.props.filter : "",
            loading: false,
            showResults: false
        };
    }

    public render() {
        return (
            <div className="form-group form-group-lg">
                <RefinementInput ref={this.inputRef} filter={this.state.filter} onChange={(e) => this.keywordsChanged(e)} placeholder="Search by categories..." />
            </div>
        );
    }

    public componentDidMount = () => {
        this.focusInput()
    }

    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (prevState.filter !== this.state.filter) {
            this.props.filterChanged(this.state.filter);
        }
    }

    public focusInput = () => {
        // if (this.inputRef.current) {
        //     this.inputRef.current.focus();
        // }
    }

    private keywordsChanged = (e: React.ChangeEvent<HTMLInputElement>) => {
        const filter = e.target.value;

        this.setState({
            filter: filter
        });
    }

}