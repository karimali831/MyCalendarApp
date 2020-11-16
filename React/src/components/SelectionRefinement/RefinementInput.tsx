import * as React from 'react'

interface IOwnProps {
    onChange: (e: React.ChangeEvent<HTMLInputElement>) => void
    placeholder: string
    filter: string
}

export class RefinementInput extends React.Component<IOwnProps> {

    private inputRef: React.RefObject<HTMLInputElement>;

    constructor(props: IOwnProps) {
        super(props);

        this.inputRef = React.createRef<HTMLInputElement>();
    }

    public focus = () => {
        if (this.inputRef.current) {
            this.inputRef.current.focus()
        }
    }

    public render = () => (
        <input
            className="form-control"
            type="text"
            ref={this.inputRef}
            value={this.props.filter}
            onChange={this.props.onChange}
            placeholder={this.props.placeholder}
        />
    )

}