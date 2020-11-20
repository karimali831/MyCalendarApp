import * as React from 'react'
import { debounce } from "lodash";


interface IOwnProps {
    placeholder: string
    filter?: string | null,
    onChange: (filter: string) => void
}

interface IOwnState {
    filter: string
}

export class RefinementInput extends React.Component<IOwnProps, IOwnState> {

    public raiseDoSearchWhenUserStoppedTyping = debounce(() => {
        if (this.state.filter != null) {
            this.props.onChange(this.state.filter);
        }
      }, 2000);

    private inputRef: React.RefObject<HTMLInputElement>;

    constructor(props: IOwnProps) {
        super(props);

        this.inputRef = React.createRef<HTMLInputElement>();
        this.state = {
            filter: ""
        };
    }
    

    public render = () => (
        <input
            className="form input100"
            type="text"
            ref={this.inputRef}
            defaultValue={(this.props.filter != null ? this.props.filter : this.state.filter)}
            placeholder={this.props.placeholder}
            onChange={this.handleCriteriaChange}
        />
    )

    public focus = () => {
        if (this.inputRef.current) {
            this.inputRef.current.focus()
        }
    }

    private handleCriteriaChange = (e: React.ChangeEvent<HTMLInputElement>) => {

        this.setState({ filter: e.target.value }, () => {
            this.raiseDoSearchWhenUserStoppedTyping();
        });
    };
}