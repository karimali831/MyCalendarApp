import * as React from 'react';
import { ISpending } from '../../../models/ISpending';
import { priceFormatter, boolHighlight } from '../../utils/Utils';
import Table from '../../base/CommonTable';
import { ITableProps, ITableOptions } from 'react-bootstrap-table-next';
import { LoadSpendingsAction } from 'src/state/contexts/spending/Actions';
import { Load } from '@appology/react-components';


export interface IPropsFromState {
    spendings: ISpending[],
    loading: boolean
}

export interface IPropsFromDispatch {
    loadSpendings: typeof LoadSpendingsAction.creator
}

type AllProps = IPropsFromState & IPropsFromDispatch;

const columns: ITableProps[] = [{
    dataField: 'id',
    text: '#',
    hidden: true
}, {
    dataField: 'name',
    text: 'Item'
}, {
    dataField: 'amount',
    text: 'Amount',
    formatter: priceFormatter
}, {
    dataField: 'date',
    text: 'Date',
    editor:  {
        type: 'date'
    }
}, {
    dataField: 'category',
    text: 'Category',
    headerClasses: "hidden-xs",
    classes: "hidden-xs"
}, {
    dataField: 'cashExpense',
    text: 'Cash Expense',
    headerClasses: "hidden-xs",
    classes: "hidden-xs",
    formatter: boolHighlight
}, {
    dataField: 'secondCategory',
    text: 'Second Cat',
    headerClasses: "hidden-xs",
    classes: "hidden-xs"
}, {
    dataField: 'info',
    text: 'Info',
    headerClasses: "hidden-xs",
    classes: "hidden-xs"
}
];

const options: ITableOptions = {
    deleteRow: true,
    pagination: true
}

export const Spendings : React.SFC<AllProps> = (props) => (
    <>
        {
          props.loading ?
            <Load withBackground={true} /> :
            <Table 
                table="Spendings"
                data={props.spendings}
                columns={columns}
                options={options}
            /> 
        }
    </>
)

export default Spendings