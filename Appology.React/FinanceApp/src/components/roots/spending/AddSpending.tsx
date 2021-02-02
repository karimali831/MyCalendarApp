import * as React from 'react';
import { Redirect } from 'react-router-dom'
import * as moment from 'moment'
import { IFinance } from '../../../models/IFinance';
import { commonApi } from '../../../Api/CommonApi';
import { api } from '../../../Api/Api';
import { Load } from '../../base/Loader';
import { ISpendingDTO } from '../../../models/ISpending';
import SelectionRefinementForSpendingCategories from './SelectionRefinementForSpendingCategories';
import { appPathUrl } from 'src/components/utils/Utils';


export interface IPropsFromState {
    selectedCat?: number,
    selectedSecondCat?: number,
}


export interface IPropsFromDispatch {
}

export interface IOwnState {
    finances: IFinance[],
    amount?: number | undefined,
    loading: boolean,
    name: string,
    date: string,
    cashExpense: boolean,
    selectedFinanceId: number | undefined
    redirect: boolean
}


type AllProps = IPropsFromState & IPropsFromDispatch;

class AddSpending extends React.Component<AllProps, IOwnState> {
    constructor(props: AllProps) {
        super(props);
        this.state = { 
            finances: [],
            amount: undefined,
            loading: true,
            name: "",
            date: moment(new Date()).format('YYYY-MM-DDTHH:mm'),
            cashExpense: false,
            selectedFinanceId: undefined,
            redirect: false
        };
    }

    public componentDidMount() {
        this.loadFinances();
    }
    
    public render() {
        const { redirect, loading } = this.state;

        if (redirect) {
            return <Redirect to={`${appPathUrl}/home`} />;
        }

        if (loading) {
            return <Load text="Loading..." />
        }

        return (
            <div>
                <form className="form-horizontal">
                    <div className="form-group form-group-lg">
                        <input className="form-control" type="text" value={this.state.name} placeholder="Enter item" onChange={(e) => { this.onSpendingChanged(e);}} />
                    </div>
                    <div className="form-group form-group-lg">
                        <input className="form-control" type="datetime-local" value={this.state.date} placeholder="dd-MM-yy" onChange={(e) => { this.onDateChanged(e);}} />
                    </div>
                    <div className="form-group form-group-lg">
                        <select onChange={e => this.onChangeSelectedFinance(e)} className="form-control">
                            <option value={"undefined"}>-- of finance --</option>
                            {
                                this.state.finances.map(f =>
                                    <option key={f.id} value={f.id + "-" + f.name + "-" + f.avgMonthlyAmount}>{f.name}</option>
                                )
                            }
                        </select>
                    </div>
                    {this.state.selectedFinanceId === undefined || this.state.selectedFinanceId === 0 ? 
                        <div className="form-group form-group-lg">
                            <SelectionRefinementForSpendingCategories />
                        </div>
                    : null}
                    <div className="form-group form-group-lg">
                        <input className="form-control" type="number" value={this.state.amount} placeholder="Enter amount" onChange={(e) => { this.onAmountChanged(e);}} />
                    </div>
                    <div className="form-group form-group-lg">
                        <input className="form-control" type="checkbox" defaultChecked={this.state.cashExpense} onChange={(e) => { this.onCashExpenseChanged();}} /> Cash Expense?
                    </div>
                    <button className="btn btn-primary" onClick={() => this.addSpending() }>Add Spending</button>
                </form>
            </div>
        )
    }


    private loadFinances = () => {
        api.finances()
            .then(response => this.loadFinancesSuccess(response.finances));
    }

    private loadFinancesSuccess = (finances: IFinance[]) => {
        this.setState({ ...this.state,
            ...{ 
                loading: false, 
                finances: finances
            }
        }) 
    }

    private onSpendingChanged =  (e: React.ChangeEvent<HTMLInputElement>) => {
        this.setState({ ...this.state, name: e.target.value }) }

    private onCashExpenseChanged =  () => {
        this.setState({ ...this.state, cashExpense: !this.state.cashExpense }) }

    private onDateChanged =  (e: React.ChangeEvent<HTMLInputElement>) => {
        this.setState({ ...this.state, date: e.target.value })  
    }

    private onAmountChanged =  (e: React.ChangeEvent<HTMLInputElement>) => {
        this.setState({ ...this.state, amount: Number(e.target.value) })  
    }

    private onChangeSelectedFinance = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const value = e.target.value;
        const finance = value.split("-", 3);

        this.setState({ ...this.state, 
            ...{ 
                selectedFinanceId: finance[0] === "undefined" ? undefined : Number(finance[0]),
                amount: Number(finance[2]),
                name: finance[1]
            }
        })  
    }

    private addSpending = () => {
        if (this.state.name && this.state.name.length > 2 && (this.props.selectedCat || this.state.selectedFinanceId) && (this.state.amount || this.state.amount === 0))
        {
            const addModel: ISpendingDTO = {
                name: this.state.name,
                catId: this.state.selectedFinanceId !== undefined ? null : this.props.selectedCat,
                secondCatId: this.state.selectedFinanceId !== undefined ? null : this.props.selectedSecondCat,
                amount: this.state.amount,
                date: this.state.date,
                financeId: this.state.selectedFinanceId,
                cashExpense: this.state.cashExpense
            }
            
            commonApi.add(addModel, "spendings");
            this.setState({ ...this.state, redirect: true })
        }
        else{
            alert("Enter item and amount first");
        }
    }
}

export default AddSpending