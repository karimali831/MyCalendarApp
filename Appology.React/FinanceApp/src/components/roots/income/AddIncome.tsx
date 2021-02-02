import * as React from 'react';
import { commonApi } from '../../../Api/CommonApi'
import { Redirect } from 'react-router-dom'
import { IIncomeDTO } from '../../../models/IIncome';
import SelectionRefinementForIncomeSources from './SelectionRefinementForIncomeSourcesConnected';
import * as moment from 'moment';
import { appPathUrl } from 'src/components/utils/Utils';

export interface IPropsFromState {
    selectedCat?: number,
    selectedSecondCat?: number,
}

export interface IOwnState {
    date: string,
    name: string,
    amount?: number | undefined
    redirect: boolean
}

export default class AddExpense extends React.Component<IPropsFromState, IOwnState> {
    constructor(props: IPropsFromState) {
        super(props);
        this.state = { 
            name: "",
            amount: undefined,
            date: moment(new Date()).format('YYYY-MM-DDTHH:mm'),
            redirect: false
        };
    }

    public render() {
        const { redirect } = this.state;

        if (redirect) {
            return <Redirect to={`${appPathUrl}/income/`} />;
        }

        return (
            <div>
                <form className="form-horizontal">
                    <div className="form-group form-group-lg">
                        <input className="form-control" type="text" value={this.state.name} placeholder="Enter item" onChange={(e) => { this.onNameChanged(e);}} />
                    </div>
                    <div className="form-group form-group-lg">
                        <SelectionRefinementForIncomeSources />
                    </div>
                    <div className="form-group form-group-lg">
                        <input className="form-control" type="datetime-local" value={this.state.date} placeholder="dd-MM-yy" onChange={(e) => { this.onDateChanged(e);}} />
                    </div>
                    <div className="form-group form-group-lg">
                        <input className="form-control" type="number" value={this.state.amount} placeholder="Enter amount" onChange={(e) => { this.onAmountChanged(e);}} />
                    </div>
                    <button className="btn btn-primary" onClick={() =>this.addExpense() }>Add Earning</button>
                </form>
            </div>
        )
    }

    private onNameChanged =  (e: React.ChangeEvent<HTMLInputElement>) => {
        this.setState({ ...this.state, 
            ...{ 
                name: e.target.value,
            }
        })  
    }
    
    private onDateChanged =  (e: React.ChangeEvent<HTMLInputElement>) => {
        this.setState({ ...this.state, date: e.target.value })  
    }

    private onAmountChanged =  (e: React.ChangeEvent<HTMLInputElement>) => {
        this.setState({ ...this.state,  amount: Number(e.target.value) })  
    }

    private addExpense = () => {
        if (this.state.date  && this.props.selectedCat && (this.state.amount || this.state.amount === 0))
        {
            const addModel: IIncomeDTO = {
                name: this.state.name,
                sourceId: this.props.selectedCat,
                secondSourceId: this.props.selectedSecondCat,
                date: this.state.date,
                amount: this.state.amount
            }

            commonApi.add(addModel, "incomes");
            this.setState({ ...this.state, redirect: true })  
        }
        else{
            alert("Enter date, amount and select income source");
        }
    }
}