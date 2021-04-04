import Form from 'react-bootstrap/Form'
import * as React from 'react';
import Button from 'react-bootstrap/Button'
import { FaAngleDoubleRight, FaInfoCircle, FaMinus, FaPlus, FaPlusSquare, FaShoppingBasket, FaStickyNote, FaTimes, FaTrashAlt, FaUser } from 'react-icons/fa';
import { OrderOverview } from './OrderOverview';
import { IOrderForm, IOrderItem } from 'src/models/IOrder';
import { IDefaultConfig } from 'src/models/IDefaultConfig';
import { ITripOverview } from 'src/models/ITrip';
import { ToggleConfigAction, UpdateOrderAction } from 'src/state/contexts/order/Actions';
import { IOrderOverview } from 'src/models/IOrder';
import { ActionButton } from 'src/components/utils/ActionButtons';
import { api } from 'src/Api/Api';
import { Variant, Delayed } from '@appology/react-components';
import Badge from 'react-bootstrap/Badge'
import { ResetOrderAction, SetActiveStepAction, ToggleAlertAction } from 'src/state/contexts/landing/Actions';
import { IPlace, IPlaceItemSearch } from 'src/models/IPlace';
import Alert from 'react-bootstrap/Alert'
import { OrderItemsSearch } from './OrderItemsSearch';
import Container from 'react-bootstrap/Container'
import Row from 'react-bootstrap/Row'
import Col from 'react-bootstrap/Col'
import { ActionDialogue } from 'src/Enums/ActionDialogue';

export interface IPropsFromDispatch {
    toggleConfig: () => ToggleConfigAction,
    updateOrder: (order: IOrderForm) => UpdateOrderAction,
    setActiveStep: (step: number) => SetActiveStepAction,
    handleAlert: (text: string, variant?: Variant, timeout?: number) => ToggleAlertAction,
    resetOrder: () => ResetOrderAction
}

export interface IPropsFromState {
    tripOverview?: ITripOverview,
    orderOverview: IOrderOverview,
    order: IOrderForm,
    config: IDefaultConfig,
    pinSidebar: boolean,
    place?: IPlace
}

export interface IOwnState {
    deleting: boolean,
    deletingRef?: number,
    addNoteTblRef?: number,
    noApiMsgShow: boolean
}

export interface IOwnProps {
    confirmAction?: boolean,
    actionDialogue?: ActionDialogue,
    confirmationHandled: () => void,
    showConfirmation: (actionDialogue: ActionDialogue, variant: Variant, bodyContent: JSX.Element) => void
}

type AllProps = IPropsFromState & IPropsFromDispatch & IOwnProps;

export default class OrderDetails extends React.Component<AllProps, IOwnState> {

    constructor(props: AllProps) {
        super(props);
        this.state = {
            deleting: false,
            deletingRef: undefined,
            addNoteTblRef: undefined,
            noApiMsgShow: true
        };
    }

    public componentDidUpdate = (prevProps: AllProps, prevState: IOwnState) => {
        if (JSON.stringify(this.props.order.items.filter(i => i.qty)) !== JSON.stringify(prevProps.order.items.filter(i => i.qty))) {
            this.updateFigures();
        }
        
        if (JSON.stringify(prevProps.config.orderFeeFormula) !== JSON.stringify(this.props.config.orderFeeFormula)  || prevProps.order.orderValue !== this.props.order.orderValue) {
            this.orderFeeChange();
        }

        // selected order
        if (this.props.order.orderValue !== 0 && this.props.order.orderId) {
            if (this.props.order.orderFee === 0) {
                this.orderFeeChange();
            }
        }

        if (prevProps.confirmAction !== this.props.confirmAction) {
            if (this.props.actionDialogue === ActionDialogue.RemoveItem && this.state.deletingRef !== undefined) {

                if (this.props.confirmAction) {
                    this.handleRemove(this.state.deletingRef)
                }
                else{
                    this.setState({ deletingRef: undefined })
                }
            }
            else if (this.props.actionDialogue === ActionDialogue.ClearBasket && this.props.confirmAction) {
                this.clearBasketConfirmed();
            }
            else if (this.props.actionDialogue === ActionDialogue.DeleteOrder && this.props.confirmAction) {
                this.deleteOrderConfirmed();
            }

            if (!this.props.confirmAction) {
                this.props.confirmationHandled();
            }
        }
    }

    public render() {
        return ( 
            <div>
                    {this.props.place && <OrderItemsSearch place={this.props.place} itemSelected={this.itemSelected} />}
                    {
                        this.state.noApiMsgShow && this.props.place === undefined &&
                        <Delayed waitBeforeShow={1000}>
                            <Alert variant="info">
                                <FaInfoCircle /> This store currently has no searchable product API configured
                                <span className="float-right" onClick={() => this.setState({ noApiMsgShow: false })}><FaTimes /></span>
                            </Alert>
                        </Delayed>
                }   
                {
                    !this.props.place || (this.props.place && this.props.order.items[0].name !== "") ?

                        <Container className="order-items">
                            <div className="basket-title border-bottom">
                                <span className="basket-title-header"><FaShoppingBasket /> Basket</span>
                                {
                                    this.props.order.items.length > 1 &&

                                    <span className="basket-title-clear" onClick={() => this.clearBasket()}>
                                        <Badge variant="danger"><FaTimes /> Clear</Badge>
                                    </span>
                                }
                            </div>
                            {
                                this.props.order.items.map((x, i) => {
                                    return (
                                        <>
                                            <Row className="order-item border-bottom">
                                                <Col xs="12" md="6" ld="8">
                                                    {
                                                        this.props.place ? 
                                                            <><img src={x.image} height="32" width="32" /> {x.name}</>
                                                        :
                                                            <Form.Control 
                                                                autoFocus={i === 0}
                                                                required={true}
                                                                placeholder="Item name"
                                                                readonly={this.props.place}
                                                                name="name"
                                                                value={x.name}
                                                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => this.handleInputChange(e, i)} />
                                                    }
                                                </Col>
                                                <Col xs="6" md="3" ld="2">
                                                    <div className="quantity-controls">
                                                        <div className="quantity-buttons">
                                                            <span className="quantity-display"><Badge variant="secondary">x{x.qty}</Badge> </span>
                                                            <FaMinus className="quantity-minus" onClick={() => this.handleQtyChange(i, false)} />
                                                            <FaPlusSquare className="quantity-plus" onClick={() => this.handleQtyChange(i, true)}/>
                                                            <FaStickyNote style={{ color: x.notes && "#09c" }} className="add-note" onClick={() => this.setState({ addNoteTblRef: this.state.addNoteTblRef === i ? undefined : i })} />
                                                        </div>
                                                    </div>
                                                </Col>
                                                <Col xs="6" md="3" ld="2">
                                                    <div className="float-right" >
                                                        {
                                                            this.props.place ? <span className="item-price">£{(x.cost*x.qty).toFixed(2)}</span> :
                                                            <Form.Control
                                                                required={true}
                                                                readonly={this.props.place}
                                                                name="cost"
                                                                placeholder="Price"
                                                                type="number"
                                                                value={x.cost}
                                                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => this.handleInputChange(e, i)} 
                                                            />
                                                        }
                                                    </div>
                                                </Col>
                                                {
                                                    this.state.addNoteTblRef !== undefined && this.state.addNoteTblRef === i &&
                                                    <Col xs="12" md="12">
                                                        <Form.Control 
                                                            name="notes"
                                                            placeholder="Add a note..."
                                                            value={x.notes}
                                                            onChange={(e: React.ChangeEvent<HTMLInputElement>) => this.handleInputChange(e, i)}  
                                                        />  
                                                    </Col>
                                                }
                                            </Row>
                                            {
                                                this.props.order.items.length - 1 === i && 
                                                    <Row className="order-items-controls">
                                                        <Col>
                                                            {
                                                                this.props.place === undefined || this.props.place.allowManual ?
                                                                <Button variant="success" onClick={() => this.handleAddClick}>
                                                                    <FaPlus /> Add Item
                                                                </Button>
                                                                : null
                                                            }
                                                        </Col>
                                                        <Col>
                                                            <Button className="float-right" disabled={this.props.order.orderValue < this.props.config.minimumOrderValue} variant="primary" onClick={() => this.props.setActiveStep(3)}>
                                                                <FaUser /> Assign Driver <FaAngleDoubleRight />
                                                            </Button>
                                                        </Col>
                                                    </Row>
                                            }  
                                        </>
                                    );
                                })}
                        </Container>

                    : null
                }
                {
                    this.props.tripOverview !== undefined && this.props.orderOverview !== undefined ?
                        <OrderOverview
                            order={this.props.order}
                            mov={this.props.config.minimumOrderValue-this.props.order.orderValue}
                            orderOverview={this.props.orderOverview}
                            trip={this.props.tripOverview}
                            toggleConfig={() => this.props.toggleConfig()} />
                        : null
                }
                {
                    this.props.order.orderId && 
                    <span className="float-right">
                        <ActionButton
                            icon={<FaTrashAlt />}
                            value="Delete Order" 
                            variant={Variant.Danger}
                            loading={this.state.deleting} 
                            onClick={() => this.deleteOrder()} />
                    </span>
                }
            </div>
        );
    }

    private clearBasket = () => {
        this.props.showConfirmation(
            ActionDialogue.ClearBasket,
            Variant.Warning,
            <>Are you sure you want to clear your basket?</>
        )
    }

    private clearBasketConfirmed = () => {
        this.props.updateOrder({ ...this.props.order, 
            items: [{
                name: "",
                qty: 1,
                cost: 0,
                notes: "",
                maxQuantity: 10
            }],
        });
        this.props.confirmationHandled();
    }

    private itemSelected = (item: IPlaceItemSearch) => {
        const tableItem : IOrderItem = { 
            name: item.name,
            qty: 1,
            cost: item.price,
            notes: "",
            maxQuantity: item.maxQuantity,
            image: item.leftImage
        }

        // replace object in first initialised default state
        if (this.props.order.items[0].name === "") {
            this.props.updateOrder({ ...this.props.order, items: this.props.order.items.map(i => tableItem)});
        }
        else{

            const itemAlreadyAdded = this.props.order.items.find(x => x.name === item.name);

            if (itemAlreadyAdded) {
                this.props.handleAlert("Item already added to basket", Variant.Primary)
            }
            else{
                this.props.updateOrder({ ...this.props.order, items: [...this.props.order.items, tableItem]});
            }
        }
    }

    private deleteOrder = () => {
        this.props.showConfirmation(
            ActionDialogue.DeleteOrder,
            Variant.Warning,
            <>Are you sure you want to permanently delete this order? This action is not recoverable.</>
        )
    }

    private deleteOrderConfirmed = () => {
        this.setState({ deleting: true })
        
        api.deleteOrder(this.props.order.orderId)
            .then(status => this.deleteOrderSuccess(status))
        
        this.props.confirmationHandled();
    }

    private deleteOrderSuccess = (status: boolean) => {
        this.setState({ deleting: false })

        if (status) {
            this.props.resetOrder();
            this.props.handleAlert("Order successfully deleted")
        }
        else{
            this.props.handleAlert("There was an issue deleting this order", Variant.Danger)
        }

    }

    private orderFeeChange = () => {
        let orderFee = 0;
        this.props.config.orderFeeFormula.map(c => {
            if (this.props.order.orderValue >= c.orderValueMin && this.props.order.orderValue <= c.orderValueMax) {
                orderFee = c.fee;
            }
        });

        this.props.updateOrder({ ...this.props.order,
            orderFee: this.props.order.orderValue * orderFee
        } as IOrderForm);
    }

    private handleQtyChange = (idx: number, plus: boolean) => {
        const items = [...this.props.order.items]
        const qnt = plus ? items[idx]["qty"] + 1 : items[idx]["qty"] - 1;

        if (qnt > items[idx]["maxQuantity"]) {
            this.props.handleAlert(`Maximum quantity ${items[idx]["maxQuantity"]} reached for this item`, Variant.Info)
        }
        else if (qnt === 0) {

            if (items.length === 1) {
                this.clearBasket();
            }
            else{

                this.setState({ deletingRef: idx })

                this.props.showConfirmation(
                    ActionDialogue.RemoveItem,
                    Variant.Warning,
                    <>Remove this item from your basket?</>,
                )
            }
        }
        else{
            items[idx]["qty"] = qnt;

            this.props.updateOrder({ ...this.props.order,
                items: items
            } as IOrderForm);

            this.updateFigures();
        }
    }


    private handleInputChange = (e: React.ChangeEvent<HTMLInputElement>, idx: number) => {
        const { name, value } = e.target;
        const items = [...this.props.order.items]
        items[idx][name] = value;

        this.props.updateOrder({ ...this.props.order,
            items: items
        } as IOrderForm);

        if (name === "cost") {
            this.updateFigures();
        }
    }

    private updateFigures = () => {
        this.props.updateOrder({ ...this.props.order,
            totalItems: this.props.order.items.reduce((sum, current) => sum + (current.qty * 1), 0),
            orderValue: this.props.order.items.map(item => item.qty * item.cost).reduce((a, b) => a + b)
        } as IOrderForm);
    }


    private handleRemove = (idx: number) => {
        const items = [...this.props.order.items]
        items.splice(idx, 1);

        this.props.updateOrder({ ...this.props.order,
            items: items
        } as IOrderForm);

        this.props.confirmationHandled();
    };

    private handleAddClick = () => {
        this.props.updateOrder({ ...this.props.order,
            items: [
                ...this.props.order.items, { 
                    name: "",
                    qty: 1,
                    cost: 0,
                    notes: "",
                    maxQuantity: 10
                }
            ]
        } as IOrderForm);
    };
}