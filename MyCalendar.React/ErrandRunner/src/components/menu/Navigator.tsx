import * as React from 'react';
import Stepper from '@material-ui/core/Stepper';
import Step from '@material-ui/core/Step';
import StepLabel from '@material-ui/core/StepLabel';

interface IOwnState {
    activeStep: number,
    steps: string[]
}

interface IOwnProps {
    activeStep: number,
    customerName?: string,
    pickupName: string,
    service?: string,
    setActiveStep: (step: number) => void
}


export class Navigator extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            activeStep: this.props.activeStep,
            steps: ['Customer', 'Service & Store', 'Order', 'Driver','Payment']
        };
    }

    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (this.props.activeStep !== prevState.activeStep) {
            this.setStep(this.props.activeStep)
        }
    }

    public render() {
        return (
            <div className="navigator">
                <Stepper activeStep={this.state.activeStep} alternativeLabel={true}>
                    {this.state.steps.map((label, idx) => (
                    <Step key={label}>

                        <StepLabel onClick={(e) => this.setStep(idx)}>{label}</StepLabel>
                    </Step>
                    ))}
                </Stepper>
            </div>
          );
    }

    private setStep = (idx: number) => {
        let disabled : boolean = false;

        if (this.state.activeStep === 0 && this.props.customerName === undefined) {
            disabled = true;
            alert("Select or register a customer first");
        }

        else if (this.state.activeStep === 1 && idx > this.state.activeStep) {
            if (this.props.pickupName === "" || this.props.service === undefined) {
                disabled = true;
                alert("Select a service and store first");
            }
        }

        // else if (this.state.activeStep === 2 && idx > this.state.activeStep) {
            
        // }

        if (!disabled) {
            const labels = [...this.state.steps];

            if (idx === 1) {
                labels[idx-1] = `Deliver to ${this.props.customerName}`
            }
            else if (idx === 2) {
                labels[idx-1] = `Pickup from ${this.props.pickupName}`
            }

            this.setState({ 
                activeStep: idx,
                steps: labels
            })
            this.props.setActiveStep(idx);
        }
    }
}