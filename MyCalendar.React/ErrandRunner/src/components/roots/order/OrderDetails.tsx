import Form from 'react-bootstrap/Form'
import Table from 'react-bootstrap/Table'
import * as React from 'react';
import { IOrderItem } from 'src/models/IOrderItem';
import Button from 'react-bootstrap/Button'
import { FaMinus, FaPlus, FaSlidersH } from 'react-icons/fa';
import InputGroup from 'react-bootstrap/InputGroup'
import { OrderOverview } from './OrderOverview';
import { ITripOverview } from 'src/models/TripOverview';
import { IDefaultConfig } from 'src/models/IDefaultConfig';

interface IOwnState {
    items: IOrderItem[],
    orderFee: number,
    itemsQuantity: number,
    itemsCost: number
}

interface IOwnProps {
    trip: ITripOverview,
    config: IDefaultConfig,
    pinSidebar: boolean,
    configChange: () => void
}

export class OrderDetails extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            items: [{
                name: "",
                qty: 1,
                cost: 0,
                total: 0,
                notes: ""
            }],
            orderFee: 0,
            itemsQuantity: 1,
            itemsCost: 0
        };
    }

    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (JSON.stringify(this.state.items.filter(i => i.qty)) !== JSON.stringify(prevState.items.filter(i => i.qty))) {
            this.updateFigures();
        }

        if (JSON.stringify(prevProps.config.orderFeeFormula) !== JSON.stringify(this.props.config.orderFeeFormula)  || prevState.itemsCost !== this.state.itemsCost) {
            this.orderFee();
        }
    }

    public render() {
        const tripMileage = Number(this.props.trip.distance.replace(/\D+$/g, ""));
        const calcDeliveryFee = tripMileage * this.props.config.deliveryFeePerMile;
        const serviceFee = this.props.config.serviceFee * this.state.itemsCost;
        const invoiceAmt = this.state.itemsCost + this.state.orderFee + serviceFee + calcDeliveryFee;
        const netProfit = invoiceAmt - this.state.itemsCost;
        const driverFee = netProfit * this.props.config.driverFee;

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
                            this.state.items.map((x, i) => {
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
                                                            this.state.items.length !== 1 && 
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
                                                this.state.items.length - 1 === i && 
                                                <>
                                                    <td colSpan={2}>
                                                        <Button variant="primary" onClick={this.props.configChange}>
                                                            <FaSlidersH /> {this.props.pinSidebar ? "Hide Config" : "Show Config"} 
                                                    </Button>
                                                    </td>
                                                    <td colSpan={2} align="right">
                                                        <Button variant="success" onClick={this.handleAddClick}>
                                                            <FaPlus /> Add
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
                <OrderOverview
                    itemsQuantity={this.state.itemsQuantity}
                    totalItemsCost={this.state.itemsCost}
                    serviceFee={serviceFee}
                    orderFee={this.state.orderFee}
                    deliveryFee={calcDeliveryFee}
                    tripMileage={tripMileage}
                    invoiceAmt={invoiceAmt}
                    netProfit={netProfit}
                    driverFee={driverFee}
                    trip={this.props.trip} />
            </div>
        );
    }

    private orderFee = () => {
        let orderFee = 0;

        this.props.config.orderFeeFormula.map(c => {
            if (this.state.itemsCost >= c.orderValueMin && this.state.itemsCost <= c.orderValueMax) {
                orderFee = c.fee;
            }
        });

        this.setState({ orderFee: this.state.itemsCost * orderFee })
    }

    private handleInputChange = (e: React.ChangeEvent<HTMLInputElement>, idx: number) => {
        const { name, value } = e.target;
        const items = [...this.state.items]
        items[idx][name] = value;

        if (name === "qty" || name === "cost") {
            this.updateFigures();
        }

        this.setState({ items: items });
    }

    private updateFigures = () => {
        this.setState({ 
            itemsQuantity: this.state.items.reduce((sum, current) => sum + (current.qty * 1), 0),
            itemsCost: this.state.items.map(item => item.qty * item.cost).reduce((a, b) => a + b)
        })
    }


    private handleRemoveClick = (idx: number) => {
        const items = [...this.state.items]
        items.splice(idx, 1);
        this.setState({ items: items });
    };

    private handleAddClick = () => {
        this.setState({ 
            items: [
                ...this.state.items, { 
                    name: "",
                    qty: 1,
                    cost: 0,
                    total: 0,
                    notes: ""
                }
            ]
        });
    };
}