import * as React from 'react';
import { IIncome } from '../../../models/IIncome';
import { priceFormatter } from '../../utils/Utils';
import Table from '../../base/CommonTable';
import { ITableProps, ITableOptions } from 'react-bootstrap-table-next';
import { LoadIncomesAction } from 'src/state/contexts/income/Actions';
import { Load } from '@appology/react-components';

export interface IPropsFromState {
    incomes: IIncome[],
    loading: boolean
}

export interface IPropsFromDispatch {
    loadIncomes: typeof LoadIncomesAction.creator
}

type AllProps = IPropsFromState & IPropsFromDispatch;

const columns: ITableProps[] = [{
    dataField: 'id',
    text: '#',
    hidden: true
  }, {
    dataField: 'name',
    text: 'Name'
  }, {
    dataField: 'weekNo',
    text: 'WeekNo'
  }, {
    dataField: 'source',
    text: 'Source'
  }, {
    dataField: 'date',
    text: 'Paid Date',
    editor:  {
        type: 'date'
    }
  }, {
    dataField: 'amount',
    text: 'Amount',
    formatter: priceFormatter
  }, {
    dataField: 'secondSource',
    text: 'Second Source',
    headerClasses: "hidden-xs",
    classes: "hidden-xs"
  }
];

const options: ITableOptions = {
    deleteRow: true
}

export const Incomes : React.SFC<AllProps> = (props) => (
    <>
        {
          props.loading ?
            <Load withBackground={true} /> :
            <Table 
                table="Incomes"
                data={props.incomes}
                columns={columns}
                options={options}
            /> 
        }
    </>
)
 export default Incomes