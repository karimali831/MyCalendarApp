import * as React from 'react';
import { api } from '../../../Api/Api';
import { Load } from '../../base/Loader';
import { priceFormatter, intToOrdinalNumberString, paymentStatus, cleanText, boolHighlight, spendingsForFinance } from '../../utils/Utils';
import { IFinance } from '../../../models/IFinance';
import Table from '../../base/CommonTable';
import { OverrideDueDate } from '../../../enums/OverrideDueDate';
import { ITableProps, ITableOptions } from 'react-bootstrap-table-next';
import { Link } from 'react-router-dom';
import { faSync } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import Badge from 'react-bootstrap/Badge'

interface IOwnProps {
}

export interface IOwnState {
    finances: IFinance[],
    totalAvgCost: number | undefined,
    spentThisMonth: number | undefined,
    spentLastMonth: number | undefined,
    loading: boolean,
    showEdit: number | undefined
}

export default class Finances extends React.Component<IOwnProps, IOwnState> {
    private tableName = "Finances";
    
    constructor(props: IOwnProps) {
        super(props);
        this.state = { 
            loading: true,
            finances: [],
            totalAvgCost: undefined,
            spentThisMonth: undefined,
            spentLastMonth: undefined,
            showEdit: undefined
        };
    }

    public componentDidMount() {
        this.loadFinances();
    }

    public render() {
        if (this.state.loading) {
            return <Load text="Loading finances..." />
        }

        const columns: ITableProps[] = [{
            dataField: 'id',
            text: '#',
            hidden: true
          }, {
            dataField: 'name',
            text: 'Name',
            formatter: this.spendingsForFinance
          }, {
            dataField: 'avgMonthlyAmount',
            text: 'Avg Monthly Cost',
            formatter: priceFormatter
          }, {
            dataField: 'overrideNextDueDate',
            text: 'Override Due Date',
            headerClasses: "hidden-xs",
            classes: "hidden-xs",
            formatter: this.overrideDueDate
          }, {
            dataField: 'manualPayment',
            text: 'One-Off Payment',
            headerClasses: "hidden-xs",
            classes: "hidden-xs",
            formatter: boolHighlight
          }, {
            dataField: 'monthlyDueDate',
            text: 'Due Date',
            formatter: intToOrdinalNumberString
          }, {
            dataField: 'endDate',
            text: 'End Date',
            editor:  {
                type: 'date'
            },
            headerClasses: "hidden-xs",
            classes: "hidden-xs"
          }, {
            dataField: 'daysUntilDue',
            text: 'Next Due',
            hidden: true
          }, {
            dataField: 'monzoTag',
            text: 'Monzo Tag',
            headerClasses: "hidden-xs",
            classes: "hidden-xs"
          }, {
            dataField: 'superCatId',
            text: 'Super CatId',
            headerClasses: "hidden-xs",
            classes: "hidden-xs"
          }, {
            dataField: 'directDebit',
            text: 'Is DD',
            headerClasses: "hidden-xs",
            classes: "hidden-xs",
            formatter: boolHighlight
          }, {
            dataField: 'paymentStatus',
            text: 'Status',
            formatter: this.paymentStatus
          }
        ];

        const options: ITableOptions = {
            deleteRow: true
        }

        return (
            <div>
                <div className="card">
									<div className="card-body">
									<h5 className="card-title">Finance Overview</h5>
										<p className="card-text">
                      Total average monthly cost: £{this.state.totalAvgCost}<br />
                      Spent last month: £{this.state.spentLastMonth} <br />
                      Spent this month: £{this.state.spentThisMonth}
										</p>
									</div>
									<div className="card-footer">
									<small className="text-muted">
                    <a onClick={() => this.loadFinances(true)}>
                      <FontAwesomeIcon icon={faSync} /> 
                      Re-sync next due dates
                    </a>
                  </small>
									</div>
								</div>
                <Table 
                    table={this.tableName}
                    data={this.state.finances}
                    columns={columns}
                    options={options}
                /> 
            </div>
        )
    }

    private loadFinances = (resyncNextDueDates = false) => {
        api.finances(resyncNextDueDates)
            .then(response => this.loadFinancesSuccess(response.finances, response.totalAvgCost, response.spentThisMonth, response.spentLastMonth));
    }

    private loadFinancesSuccess = (finances: IFinance[], totalAvgCost: number, spentThisMonth: number, spentLastMonth: number) => {
        this.setState({ ...this.state,
            ...{ 
                loading: false, 
                finances: finances,
                totalAvgCost: totalAvgCost,
                spentThisMonth: spentThisMonth,
                spentLastMonth: spentLastMonth
            }
        }) 
    }

    private paymentStatus = (cell: any, row: any) => {
        return paymentStatus(cell, row['daysUntilDue'])
    }

    private spendingsForFinance = (cell: any, row: any) => {
        return <Link to={spendingsForFinance(row['id'])}>{row['name']}</Link>
    }

    private overrideDueDate = (cell: any, row: any) => {
        return <Badge variant="primary">{cleanText(OverrideDueDate[cell])}</Badge>
    }
}