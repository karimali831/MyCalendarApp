import * as React from 'react';
import Stepper from '@material-ui/core/Stepper';
import Step from '@material-ui/core/Step';
import StepLabel from '@material-ui/core/StepLabel';
import { SetActiveStepAction, ToggleAlertAction } from 'src/state/contexts/landing/Actions';
import { INavigator } from 'src/models/INavigator';
import { Variant } from '@appology/react-components';

export interface IPropsFromDispatch {
    setActiveStep: (step: number) => SetActiveStepAction,
    handleAlert: (text: string, variant?: Variant, timeout?: number) => ToggleAlertAction
}

export interface IPropsFromState {
    activeStep: number,
    navigator: INavigator[]
}

export interface IOwnState {
}

type AllProps = IPropsFromState & IPropsFromDispatch;

export default class Navigator extends React.Component<AllProps, IOwnState> {

    constructor(props: AllProps) {
        super(props);
    }

    public render() {
        return (
            <div className="navigator">
                <Stepper activeStep={this.props.activeStep} alternativeLabel={true}>
                    {this.props.navigator.map(n => (
                    <Step key={n.stepNo}>
                        <StepLabel onClick={(e) => this.setStep(n.stepNo)}>{n.label}</StepLabel>
                    </Step>
                    ))}
                </Stepper>
            </div>
          );
    }

    private setStep = (idx: number) => {
        if (this.props.navigator[idx].disabledMsg !== "") {
            this.props.handleAlert(this.props.navigator[idx].disabledMsg, Variant.Danger);
        }
        else{
            this.props.setActiveStep(idx);
        }
    }
}