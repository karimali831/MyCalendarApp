import * as React from 'react'
import { IDateFilter } from 'src/models/IDateFilter';
import { DateFilterChangeAction, } from 'src/state/contexts/common/Actions';
import DateFilter from './DateFilter';
import { DataType } from 'src/enums/DataType';

export interface IPropsFromState {
    dateFilter: IDateFilter,
    dataType: DataType
}

export interface IPropsFromDispatch {
    dateFilterChanged: (filter: IDateFilter, dataType: DataType) => DateFilterChangeAction
}

type AllProps = IPropsFromState & IPropsFromDispatch

const DateFilterProps: React.SFC<AllProps> = (props) =>
    <DateFilter
        dateFilter={props.dateFilter}
        dateFilterChanged={props.dateFilterChanged}
        dataType={props.dataType}
    />

export default DateFilterProps