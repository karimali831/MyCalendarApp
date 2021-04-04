import { Variant } from '@appology/react-components';
import * as React from 'react';
import ListGroup from 'react-bootstrap/ListGroup'
import { FaCheck, FaExclamationTriangle, FaInfo, FaTimes, FaTruck } from 'react-icons/fa';
import { ActionButton } from 'src/components/utils/ActionButtons';
import { SaveStatus } from 'src/Enums/SaveStatus';
import { IDispatchStatus, OrderAction } from 'src/models/IDispatchStatus';
import { IOrder } from 'src/models/IOrder';
import { DispatchOrderAction, SaveOrderAction } from 'src/state/contexts/order/Actions';
import Button from 'react-bootstrap/Button'

export interface IPropsFromDispatch {
    dispatchOrder: (dispatch: boolean) => DispatchOrderAction,
    saveOrder: (saved?: boolean) => SaveOrderAction
}

export interface IPropsFromState {
    order?: IOrder,
    deliveryDate?: Date,
    timeslot?: string,
    saveOrderStatus: SaveStatus,
    actions: IDispatchStatus[],
    orderStatus: IDispatchStatus[]
}

export interface IOwnState{
    actions: IDispatchStatus[]
}

type AllProps = IPropsFromDispatch & IPropsFromState


export default class OrderChecklist extends React.Component<AllProps, IOwnState> {

    constructor(props: AllProps) {
        super(props);

        this.state = {
            actions: this.props.actions
        };
    }

    public componentDidMount() {
        this.props.saveOrder();
    }

    public componentDidUpdate = (prevProps: AllProps, prevState: IOwnState) => {
        if (JSON.stringify(prevProps.actions) !== JSON.stringify(this.props.actions)) {
            this.setState({ actions: this.props.actions })
        }
    }

    public render() {

        const processing : boolean = this.props.saveOrderStatus === SaveStatus.Processing;
        const dispatchNotReady : boolean = this.state.actions.some(x => x.variant !== Variant.Success && x.show);

        return (
            <ListGroup style={{ margin: -21, fontSize: "small" }}>
                {
                    this.state.actions.filter(x => x.show).map((action, i) => {
                        let variant;
                        let text;

                        if (action.variant === Variant.Warning) {
                            variant = <FaExclamationTriangle />
                            text = action.pendingTxt;
                        }
                        else if (action.variant === Variant.Success) {
                            variant = <FaCheck />
                            text = action.completeTxt;
                        }
                        else if (action.variant === Variant.Danger) {
                            variant = <FaTimes />
                            text = action.errorTxt
                        }
                        else{
                            variant = <FaInfo />
                            text = action.pendingTxt
                        }

                        return (
                            <ListGroup.Item key={i} variant={action.variant && Variant[action.variant].toLowerCase()}>
                                <span className="alert-icon">{variant}</span>
                                <span>Step {action.stepNo} · {text}</span>
                            </ListGroup.Item>
                        )
                    })
                }
                <hr />                
                {
                    this.props.orderStatus.some(x => x.orderAction === OrderAction.Dispatch && x.variant === Variant.Success) ?
                        <Button variant="success"><FaTruck /> Order Dispatched</Button> :
                        <ActionButton 
                            loading={this.props.saveOrderStatus === SaveStatus.Dispatching} 
                            icon={<FaTruck />}
                            variant={processing || dispatchNotReady ? Variant.Secondary : Variant.Danger}
                            disabled={processing || dispatchNotReady} 
                            value={`Dispatch Order ${processing || dispatchNotReady ? "(Not Ready)" : "(Ready)"}`} 
                            onClick={() => this.props.dispatchOrder(true)} 
                        />
                }
            </ListGroup>
        );
    } 
}

    // success.push("Customer selected and dropoff address set")
    // success.push("Store selected and item(s) added to basket")
    // success.push("Minimum order amount has been reached")
    // success.push("Driver has been manually assigned for the trip")
    // success.push("Pickup location is within the distance limit from driver location")
    // success.push("Dropoff location is within the distance limit from pickup location")

    // warning.push("Some actions in the basket has notes ")
    // warning.push("Currently no driver has accepted the delivery request yet")