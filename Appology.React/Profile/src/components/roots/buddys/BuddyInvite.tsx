
import * as React from 'react';
import Form from 'react-bootstrap/Form'
import Button from 'react-bootstrap/Button'
import InputGroup from 'react-bootstrap/InputGroup'
import { FaLink } from 'react-icons/fa';
import { Variant } from '@appology/react-components';

export interface IOwnProps {
    inviterShareLink: string,
    showAlert: (txt: string, variant: Variant, timeout?: number) => void
}

export interface IOwnState {
}

export class BuddyInvite extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
        };
    }

    public render() {
        return (
            <div>
                {/* {
                document.queryCommandSupported('copy') &&
                <div>
                    <button onClick={this.copyToClipboard}>Copy</button> 
                    {this.state.copySuccess}
                </div>
                } */}
         
                <InputGroup>
                    <Form.Control readOnly={true} value={this.props.inviterShareLink} />
                    <InputGroup.Prepend>                 
                        <Button variant="primary" onClick={() => this.copyToClipboard()}>
                            <FaLink /> Copy
                        </Button>
                    </InputGroup.Prepend>
                </InputGroup>
     
            </div>
        );
    }

    private copyToClipboard = () => {
        const obj = document.createElement("input");
        document.body.appendChild(obj);
        obj.setAttribute('value', this.props.inviterShareLink);
        obj.select();
        document.execCommand("copy");
        document.body.removeChild(obj);

        this.props.showAlert("Copied to clipboard!", Variant.Success)
    };
    
}