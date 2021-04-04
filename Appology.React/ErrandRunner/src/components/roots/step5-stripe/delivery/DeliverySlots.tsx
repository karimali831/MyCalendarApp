import * as React from 'react';
import * as moment from 'moment'
import ListGroup from 'react-bootstrap/ListGroup'
import { FaCalendar, FaCalendarCheck, FaTimes, FaChevronDown, FaChevronUp, FaClock } from 'react-icons/fa';
import { Load, Variant } from '@appology/react-components';
import { api, IDeliveryDateRequest } from 'src/Api/Api';
import { ToggleAlertAction } from 'src/state/contexts/landing/Actions';
import { OrderStatusAction, SetDeliveryDateAction } from 'src/state/contexts/order/Actions';
import { IOrder } from 'src/models/IOrder';
import { OrderAction } from 'src/models/IDispatchStatus';

export interface IOwnState {
    expandTimeslotsForDate?: string,
    deliveryDate?: Date,
    setting?: string,
    unsetting: boolean,
    viewTimeslots: boolean
}

export interface IOwnProps {
    selectedTimeslot?: string,
    deliveryDate?: Date,
    order?: IOrder,
    orderStatusChange: (action: OrderAction, variant: Variant, status: boolean) => OrderStatusAction,
    setDeliveryDate: (deliveryDate?: Date, timeslot?: string) => SetDeliveryDateAction,
    handleAlert: (text: string, variant?: Variant, timeout?: number) => ToggleAlertAction
}

export class DeliverySlots extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        const order = this.props.order

        this.state = {
            expandTimeslotsForDate: order?.deliveryDate !== undefined ? this.dateLabel(moment(order.deliveryDate)) : undefined,
            deliveryDate: order?.deliveryDate ?? this.props.deliveryDate,
            setting: undefined,
            unsetting: false,
            viewTimeslots: !props.selectedTimeslot
        };
    }

    public render() {
        const startDate = moment();
        const endDate = moment().add(5, 'days');
        const timeslots : string[] = [
            "10am to 12pm",
            "12pm to 2pm",
            "4pm to 6pm",
            "6pm to 8pm",
            "8pm to 10pm"
        ]

        return (
            <ListGroup style={{ margin: -21, fontSize: "small" }}>
                {
                    this.state.expandTimeslotsForDate && this.props.selectedTimeslot ?
                        <>
                            {
                                !this.state.viewTimeslots &&
                                <ListGroup.Item variant="success">
                                    <FaClock /> {this.state.expandTimeslotsForDate}, {this.props.selectedTimeslot}
                                </ListGroup.Item>
                            }
                            <ListGroup.Item onClick={() => this.unsetDeliveryDate()}>
                                {this.state.unsetting ? <Load smallSize={true} inlineDisplay={true} /> : <FaTimes />} Unset delivery date
                            </ListGroup.Item>
                            <ListGroup.Item onClick={() => this.setState({ viewTimeslots: !this.state.viewTimeslots})}>
                                {this.state.viewTimeslots ? <FaChevronUp /> : <FaChevronDown />} Change delivery date/timeslot
                            </ListGroup.Item>
                        </>
                    : null
                }
                {   
                    this.state.viewTimeslots ?
                        this.getDates(startDate, endDate).map((date, idx) => {
                            const dateLabel = this.dateLabel(date);

                            return (
                                <ListGroup.Item key={idx} onClick={() => this.setDeliveryDate(date)}>
                                    {this.state.expandTimeslotsForDate === dateLabel ? <FaCalendarCheck /> : <FaCalendar />} 
                                    <span style={{ marginLeft: 5 }}>{dateLabel}</span>
                                    <ListGroup style={{ marginTop: 15, display: this.state.expandTimeslotsForDate === dateLabel ? "block" : "none" }}>
                                        {
                                            timeslots.map(ts => 
                                                <ListGroup.Item key={ts} active={this.props.selectedTimeslot === ts} onClick={() => this.saveDeliveryDate(ts)}>
                                                    {
                                                        this.state.setting !== undefined && this.state.setting === ts ? 
                                                            <Load smallSize={true} inlineDisplay={true} /> : 
                                                            <FaClock />
                                                    } 
                                                    <span style={{ marginLeft: 5 }}>{ts}</span>
                                                </ListGroup.Item>
                                            )
                                        }
                                    </ListGroup>
                                </ListGroup.Item>
                            )
                        }) 
                    : null
                }
                {/* <small>Whilst every effort is made to ensure your delivery arrives in good time, delivery within the preferred timeslot is not guaranteed. Any delays to your order you will be notified.</small> */}
            </ListGroup>

        );
    }

    private unsetDeliveryDate = () => {
        if (this.props.order) {
            this.setState({ unsetting: true })
 
            api.unsetDeliveryDate(this.props.order?.orderId)
                .then(status => this.unsetDeliveryDateSuccess(status))
        }
    }

    private unsetDeliveryDateSuccess = (status: boolean) => {
        if (status) {
            this.props.setDeliveryDate(undefined, undefined)
            this.props.orderStatusChange(OrderAction.DeliveryDate, Variant.Warning, true)

            this.setState({ 
                unsetting: false,
                expandTimeslotsForDate: undefined,
                deliveryDate: undefined,
                viewTimeslots: true
            })
        }
        else{
            this.props.handleAlert("An error occured while unsetting delivery slot", Variant.Danger)
        }
    }

    private dateLabel = (date: moment.Moment) => {
        if (moment(date).isSame(moment(), 'day')) {
            return "Today";
        }
        else if (moment(date).isSame(moment().add(1, 'days'), 'day')) {
            return "Tomorrow";
        }
        else{
            return date.format("MMMM Do");
        }
    } 

    private setDeliveryDate = (date: moment.Moment) => {
        this.setState({ 
            deliveryDate: date.toDate(),
            expandTimeslotsForDate: this.dateLabel(date)
         })
    }

    private saveDeliveryDate = (ts: string) => {
        if (this.state.deliveryDate && this.props.selectedTimeslot !== ts && this.props.order) {
            this.setState({ setting: ts })

            const request : IDeliveryDateRequest = {
                orderId: this.props.order?.orderId,
                deliveryDate: this.state.deliveryDate,
                timeslot: ts
            }

            api.setDeliveryDate(request)
                .then(status => this.setDeliveryDateSuccess(status, ts))
        }
    }

    private setDeliveryDateSuccess = (status: boolean, ts: string) => {
        this.setState({ 
            setting: undefined, 
            viewTimeslots: false 
        })

        if (status && this.state.deliveryDate) {
            this.props.orderStatusChange(OrderAction.DeliveryDate, Variant.Success, false)
            this.props.setDeliveryDate(this.state.deliveryDate, ts)

        }
        else{
            this.props.handleAlert("An error occured while allocating delivery slot", Variant.Danger)
        }
    }

    private getDates(startDate: moment.Moment, stopDate: moment.Moment) {
        const dateArray : moment.Moment[] = [];
        let currentDate = startDate;

        while (currentDate <= stopDate) {
            dateArray.push(moment(currentDate));
            currentDate = currentDate.add(1, 'days');
        }

        return dateArray;
    }

    // private getHourlySlots(startString: string, endString: string) {

    //     const start = moment(startString, 'YYYY-MM-DD hh:mm a');
    //     const end = moment(endString, 'YYYY-MM-DD hh:mm a');

    //     // round starting minutes up to nearest 15 (12 --> 15, 17 --> 30)
    //     // note that 59 will round up to 60, and moment.js handles that correctly
    //     start.hour(Math.ceil(start.hour() / 2) * 2);
    //     // start.minutes(Math.ceil(start.minutes() / 15) * 15);

    //     const result = [];
    //     const current = moment(start);
    
    //     while (current <= end) {
    //         result.push(current.format('YYYY-MM-DD HH:mm'));
    //         current.add(2, 'hours');
    //         // current.add(15, 'minutes');
    //     }
    
    //     return result;
    // }
}