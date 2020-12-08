import * as React from 'react'
import { Load } from '../base/Loader';
import IBaseModel from './IBaseModel';

interface IOwnProps<T> {
    results: T[]
    onCancel: () => void
    itemSelected: (selectedResult: T) => void
    loading: boolean
    showResults: boolean
}

export class RefinementDropdown<T extends IBaseModel> extends React.Component<IOwnProps<T>>{

    public render() {
        return (
            <>
                {this.props.showResults || this.props.loading ?
                        <div className="refinement-dropdown">
                            {!this.props.loading ?
                                this.props.results.map(r =>
                                    <div key={r.id} className={`selection-item ${r.disabled ? "disabled" : ""}`} onClick={() => this.props.itemSelected(r)}>
                                        <label>{r.name} {r.disabled ? "(coming soon)" : ""} </label>
                                    </div>
                                ) :
                                <Load />
                            }
                        </div> :
                    null
                }
            </>
        )
    }
}