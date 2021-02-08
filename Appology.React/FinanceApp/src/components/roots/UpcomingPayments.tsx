import * as React from 'react';
import { api } from '../../Api/Api'
import { faArrowDown, faExclamationCircle } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { paymentStatus, spendingsForFinance } from '../utils/Utils';
import { IFinance } from '../../models/IFinance';
import { Load } from '@appology/react-components'
import { Link } from 'react-router-dom';


interface IOwnProps {
}

export interface IOwnState {
    upcomingPayments: IFinance[],
    loading: boolean
}

export default class UpcomingPayments extends React.Component<IOwnProps, IOwnState> {
    constructor(props: IOwnProps) {
        super(props);
        this.state = { 
            upcomingPayments: [],
            loading: true
        };
    }

    public componentDidMount() {
        this.loadUpcomingPayments();
    }

    public render() {
        if (this.state.loading) {
            return <Load />
        }

        return (
            <table className="table">
                <thead className="thead-light">
                    <tr>
                        <th scope="col" colSpan={2}>
                            Upcoming payments
                        </th>
                    </tr>
                </thead>
                <tbody>
                    {this.state.upcomingPayments.filter(p => p.paymentStatus !== 0).map(p => 
                        <tr key={p.id}>
                            <th scope="row">
                                <FontAwesomeIcon icon={p.manualPayment ? faExclamationCircle : faArrowDown} />  
                                <Link to={spendingsForFinance(p.id)}>{p.name}</Link> - £{p.avgMonthlyAmount}
                            </th>
                            <td>
                                {paymentStatus(p.paymentStatus, p.daysUntilDue)}
                            </td>
                        </tr>
                    )}
                </tbody>
            </table>
        )
    }

    private loadUpcomingPayments = () => {
        api.finances(false, true)
            .then(response => this.loadPaymentsSuccess(response.finances));
    }

    private loadPaymentsSuccess = (upcomingPayments: IFinance[]) => {
        this.setState({ ...this.state,
            ...{ 
                loading: false, 
                upcomingPayments: upcomingPayments
            }
        }) 
    }
}