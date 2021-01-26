import Form from 'react-bootstrap/Form'
import Table from 'react-bootstrap/Table'
import * as React from 'react';
import Button from 'react-bootstrap/Button'
import { FaAngleDoubleRight, FaMinus, FaPlus } from 'react-icons/fa';
import InputGroup from 'react-bootstrap/InputGroup'
import { OrderOverview } from './OrderOverview';
import { IOrderForm } from 'src/models/IOrder';
import { IDefaultConfig } from 'src/models/IDefaultConfig';
import { ITripOverview } from 'src/models/ITrip';
import { SetDriverStep4Action, ToggleConfigAction, UpdateOrderAction } from 'src/state/contexts/order/Actions';
import { IOrderOverview } from 'src/models/IOrder';

export interface IPropsFromDispatch {
    toggleConfig: () => ToggleConfigAction,
    updateOrder: (order: IOrderForm) => UpdateOrderAction,
    setDriverStep4: () => SetDriverStep4Action
}

export interface IPropsFromState {
    tripOverview?: ITripOverview,
    orderOverview: IOrderOverview,
    order: IOrderForm,
    config: IDefaultConfig,
    pinSidebar: boolean
}

export interface IOwnState {}

type AllProps = IPropsFromState & IPropsFromDispatch;

export default class OrderDetails extends React.Component<AllProps, IOwnState> {

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
    }

    public render() {
        return ( 
            <div>
                {
                    <Table responsive={true}>
                        <thead>
                            <tr>
                                <th>Item Name</th>
                                <th>Quantity</th>
                                <th>Cost</th>
                                <th>Notes</th>
                            </tr>
                        </thead>
                        <tbody>
                        {
                            this.props.order.items.map((x, i) => {
                                return (
                                    <>
                                        <tr>
                                            <td>
                                                <Form.Control 
                                                    autoFocus={i === 0}
                                                    required={true}
                                                    name="name"
                                                    value={x.name}
                                                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => this.handleInputChange(e, i)} />
                                            </td>
                                            <td>
                                                <Form.Control 
                                                    required={true}
                                                    name="qty"
                                                    type="number"
                                                    value={x.qty}
                                                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => this.handleInputChange(e, i)} />
                                            </td>
                                            <td>
                                                <Form.Control  
                                                    required={true}
                                                    name="cost"
                                                    type="number"
                                                    value={x.cost}
                                                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => this.handleInputChange(e, i)} />
                                            </td>
                                            <td>
                                                <InputGroup className="mb-2 mr-sm-2">
                                                    <Form.Control 
                                                        name="notes"
                                                        value={x.notes}
                                                        onChange={(e: React.ChangeEvent<HTMLInputElement>) => this.handleInputChange(e, i)} 
                                                    />
                                                    <InputGroup.Prepend>                 
                                                        {
                                                            this.props.order.items.length !== 1 && 
                                                                <Button variant="danger" onClick={() => this.handleRemoveClick(i)}>
                                                                    <FaMinus />
                                                                </Button>
                                                        }
                                                    </InputGroup.Prepend>
                                                </InputGroup>
                                            </td>
                                        </tr>
                                        <tr>
                                            {
                                                this.props.order.items.length - 1 === i && 
                                                <>
                                                    <td colSpan={2}>
                                                        <Button variant="success" onClick={this.handleAddClick}>
                                                            <FaPlus /> Add Item
                                                        </Button>
                                                    </td>
                                                    <td colSpan={2} align="right">
                                                        <Button disabled={this.props.order.orderValue < this.props.config.minimumOrderValue} variant="primary" onClick={this.props.setDriverStep4}>
                                                            <FaAngleDoubleRight /> Assign Driver
                                                        </Button>
                                                    </td>
                                                </>
                                            }
                                        </tr>
                                    </>
                                );
                            })
                        }
                        </tbody>
                    </Table>
                }
                {
                    this.props.tripOverview !== undefined && this.props.orderOverview !== undefined ?
                        <OrderOverview
                            order={this.props.order}
                            mov={this.props.config.minimumOrderValue-this.props.order.orderValue}
                            orderOverview={this.props.orderOverview}
                            trip={this.props.tripOverview}
                            toggleConfig={this.props.toggleConfig} />
                        : null
                }
            </div>
        );
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

    private handleInputChange = (e: React.ChangeEvent<HTMLInputElement>, idx: number) => {
        const { name, value } = e.target;
        const items = [...this.props.order.items]
        items[idx][name] = value;

        this.props.updateOrder({ ...this.props.order,
            items: items
        } as IOrderForm);

        if (name === "qty" || name === "cost") {
            this.updateFigures();
        }
    }

    private updateFigures = () => {
        this.props.updateOrder({ ...this.props.order,
            totalItems: this.props.order.items.reduce((sum, current) => sum + (current.qty * 1), 0),
            orderValue: this.props.order.items.map(item => item.qty * item.cost).reduce((a, b) => a + b)
        } as IOrderForm);
    }


    private handleRemoveClick = (idx: number) => {
        const items = [...this.props.order.items]
        items.splice(idx, 1);

        this.props.updateOrder({ ...this.props.order,
            items: items
        } as IOrderForm);
    };

    private handleAddClick = () => {
        this.props.updateOrder({ ...this.props.order,
            items: [
                ...this.props.order.items, { 
                    name: "",
                    qty: 1,
                    cost: 0,
                    notes: ""
                }
            ]
        } as IOrderForm);
    };
}