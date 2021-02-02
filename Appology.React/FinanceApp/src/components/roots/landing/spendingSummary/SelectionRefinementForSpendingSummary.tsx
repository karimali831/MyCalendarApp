import * as React from 'react'
import { SelectionRefinement } from 'src/components/SelectionRefinement/SelectionRefinement';
import { ISpendingSummary } from 'src/models/ISpending';
import { FilterChangedAction } from 'src/state/contexts/landing/Actions';

export interface IPropsFromState {
    filter: string | undefined
}

export interface IPropsFromDispatch {
    filterChanged: (filter: string) => FilterChangedAction
}

type AllProps = IPropsFromState & IPropsFromDispatch

const SelectionRefinementForSpendingSummary: React.SFC<AllProps> = (props) =>
    <SelectionRefinement<ISpendingSummary>
        filterChanged={props.filterChanged}
        filter={props.filter} />

export default SelectionRefinementForSpendingSummary;