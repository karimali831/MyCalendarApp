import * as React from 'react';
import { IReminder } from '../../../models/IReminder';
import Table from '../../base/CommonTable';
import { ITableProps, ITableOptions } from 'react-bootstrap-table-next';
import { api } from 'src/Api/Api';
import { Load } from '@appology/react-components'

interface IOwnProps {
}

export interface IOwnState {
    reminders: IReminder[],
    loading: boolean
}

export interface IPropsFromDispatch {}

export default class Reminders extends React.Component<IOwnProps, IOwnState> {
    private tableName = "Reminders";
    
    constructor(props: IOwnProps) {
        super(props);
        this.state = { 
            loading: true,
            reminders: []
        };
    }

    public componentDidMount() {
        this.loadReminders();
    }

    public render() {
        if (this.state.loading) {
            return <Load withBackground={true} />
        }

        const columns: ITableProps[] = [{
            dataField: 'id',
            text: '#',
            hidden: true
        }, {
            dataField: 'notes',
            text: 'Notes'
        }, {
            dataField: 'dueDate',
            text: 'Due Date',
            editor:  {
                type: 'datetime-local'
            }
        }, {
            dataField: 'display',
            text: 'Display',
            headerClasses: "hidden-xs",
            classes: "hidden-xs"
        }, {
            dataField: 'priority',
            text: 'Priority'
        }, {
            dataField: 'category',
            text: 'Category'
        }];
        
        const options: ITableOptions = {
            deleteRow: true
        }

        return (
            <div>
                <Table 
                    table={this.tableName}
                    data={this.state.reminders}
                    columns={columns}
                    options={options}
                /> 
            </div>
        )

    }

    private loadReminders = () => {
        api.reminders()
            .then(response => this.loadRemindersSuccess(response.reminders));
    }

    private loadRemindersSuccess = (reminders: IReminder[]) => {
        this.setState({ ...this.state,
            ...{ 
                loading: false, 
                reminders: reminders
            }
        }) 
    }
}