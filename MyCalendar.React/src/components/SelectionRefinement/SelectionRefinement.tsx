import * as React from 'react'
import IBaseModel from './IBaseModel';
import { RefinementDropdown } from './RefinementDropdown';
import { RefinementInput } from './RefinementInput';

interface IOwnState {
    filter?: string,
    showResults: boolean
}

interface IOwnProps<T> {
    filter: string,
    label: string,
    placeholder: string,
    filteredResults: T[],
    setFilterToItemName: boolean,
    loading: boolean,
    itemSelected: (item: T) => void,
    onCancel?: () => void,
    onChange: (filter: string) => void
}


export class SelectionRefinement<T extends IBaseModel> extends React.Component<IOwnProps<T>, IOwnState> {

    private inputRef = React.createRef<RefinementInput>();

    constructor(props: IOwnProps<T>) {
        super(props);

        this.state = {
            filter: this.props.filter !== undefined ? this.props.filter : "",
            showResults: false
        };
    }
    public render() {
        return (
            <div className="wrap-input100 validate-input m-b-23 selection-refinement">
                <span className="label-input100">{this.props.label}</span>
                <div className={this.state.showResults ? "refinement-search" : ""}>
                    <RefinementInput ref={this.inputRef} filter={this.state.filter} placeholder={this.props.placeholder} onChange={this.props.onChange} />
                    <span className="focus-input100" data-symbol="&#xf1c3;" />
                    <RefinementDropdown<T> results={this.props.filteredResults} showResults={this.state.showResults} onCancel={this.onCancel} itemSelected={this.itemSelected} loading={this.props.loading} />
                
                </div>
            </div>


        );
    }

    public componentDidMount = () => {
        this.focusInput()
    }

    public componentDidUpdate = (prevProps: IOwnProps<T>, prevState: IOwnState) => {
        if (JSON.stringify(this.props.filteredResults.map(i => i.id)) !== JSON.stringify(prevProps.filteredResults.map(i => i.id))) {
            this.setState({
                showResults: this.props.filteredResults.length > 0,
                filter: this.props.filter !== undefined ? this.props.filter : this.state.filter
            })
        }
    }


    private focusInput = () => {
        if (this.inputRef.current) {
            this.inputRef.current.focus();
        }
    }

    private itemSelected = (selectedItem: T) => {
        this.focusInput();

        if (selectedItem && !selectedItem.disabled) {
            this.props.itemSelected(selectedItem);

            this.setState({
                filter: this.props.setFilterToItemName ? selectedItem.name : "",
                showResults: false
            });
        }
    }

    private onCancel = () => {
        this.setState({
            filter: "",
            showResults: false
        });

        if (this.props.onCancel) {
            this.props.onCancel();
        }
    }
}