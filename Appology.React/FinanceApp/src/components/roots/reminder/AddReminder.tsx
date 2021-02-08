import * as React from 'react';
import { commonApi } from '../../../Api/CommonApi'
import { Redirect } from 'react-router-dom'
import { Load } from '@appology/react-components'
import { IReminderDTO } from '../../../models/IReminder';
import { Priority } from 'src/enums/Priority';

import SelectionRefinementForReminderCategories from './SelectionRefinementForReminderCategories';
import { appPathUrl, cleanText } from '../../utils/Utils';

export interface IPropsFromState {
    selectedCat?: number
}

export interface IOwnState {
    loading: boolean,
    redirect: boolean,
    notes: string,
    dueDate?: string | null,
    priority: string
}

export default class AddReminder extends React.Component<IPropsFromState, IOwnState> {
    constructor(props: IPropsFromState) {
        super(props);
        this.state = { 
            loading: false,
            redirect: false,
            notes: "",
            dueDate: null,
            priority: Priority[Priority.Low]
        };
    }

    public render() {
        const { redirect, loading } = this.state;

        if (redirect) {
            return <Redirect to={`${appPathUrl}/home`} />;
        }

        if (loading) {
            return <Load />
        }

        return (
            <div>
                <form className="form-horizontal">
                    <div className="form-group form-group-lg">
                        <label htmlFor="dueDate" className="control-label">Due Date</label>
                        <input id="dueDate" className="form-control" type="datetime-local" value={this.state.dueDate!}  onChange={(e) => { this.onDueDateInputChanged(e);}} />
                    </div>
                    <div className="form-group form-group-lg">
                        <label htmlFor="notes" className="control-label">Notes</label>
                        <textarea id="notes" className="form-control" cols={40} rows={4} onChange={(e) => { this.onNotesInputChanged(e);}} >{this.state.notes}</textarea>
                    </div>
                    <div className="form-group form-group-lg">
                        <label htmlFor="categories" className="control-label">Type</label>
                        <SelectionRefinementForReminderCategories />
                    </div>
                    <div className="form-group form-group-lg">
                        <label htmlFor="priority" className="control-label">Priority</label>
                        <select id="priority" onChange={(e) => this.onChangeSelectedPriority(e)} className="form-control">
                        {
                            Object.keys(Priority).filter(o => !isNaN(o as any)).map(key => 
                                <option key={key} value={Priority[key]} selected={this.state.priority === Priority[key]}>
                                    {cleanText(Priority[key])}
                                </option>
                            )
                        }
                        </select>
                    </div>
                    <button className="btn btn-primary" onClick={() =>this.addReminder() }>Add Reminder</button>
                </form>
            </div>
        )
    }

    private onChangeSelectedPriority = (e: React.ChangeEvent<HTMLSelectElement>) => {
        this.setState({ ...this.state,
            ...{
                priority: Priority[e.target.value]
            }
        })
    }

    private onNotesInputChanged = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
        this.setState({ ...this.state, notes: e.target.value })
    }

    private onDueDateInputChanged = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        this.setState({ ...this.state, dueDate: value === "" ? null : value })
    }

    private addReminder = () => {
        if (this.state.notes && this.state.notes.length > 2 && this.props.selectedCat && this.state.priority)
        {
            const addModel: IReminderDTO = {
                notes: this.state.notes,
                dueDate: this.state.dueDate,
                priority: Priority[this.state.priority],
                catId: this.props.selectedCat
            }

            commonApi.add(addModel, "reminders");
            this.setState({ ...this.state, redirect: true })  
        }
        else{
            alert("Enter reminder notes and due date...");
        }
    }
}