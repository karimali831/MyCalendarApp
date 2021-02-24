import * as React from 'react';
import BootstrapTable, { SelectRowOptions, ITableProps, ITableOptions } from 'react-bootstrap-table-next';
import cellEditFactory from 'react-bootstrap-table2-editor';
import paginationFactory from 'react-bootstrap-table2-paginator';
import { TableRef } from 'src/enums/TableRef';
import { commonApi, IUpdateRequest } from '../../Api/CommonApi';

export interface IOwnProps {
    table: TableRef,
    data: any,
    columns: ITableProps[],
    options?: ITableOptions,
}

export interface IOwnState {
    selectedId: number,
    deletedIds: number[]
}

export default class Table extends React.Component<IOwnProps, IOwnState> {
    constructor(props: IOwnProps) {
        super(props);
        this.state = { 
            selectedId: 0,
            deletedIds: []
        };
    }

    public render() {
        const cellEdit = cellEditFactory({
            mode: 'click',
            // blurToSave: true,
            beforeSaveCell: this.onBeforeSaveCell, // a hook for before saving cell
            afterSaveCell: this.onAfterSaveCell,  // a hook for after saving cell  
        });

        const selectRow: SelectRowOptions = {
            mode: 'radio',
            onSelect: (row: any) => {
                const field = 'id';
                this.setState({ ...this.state, 
                    ...{ 
                        selectedId: row[field]
                    }
                }) 
            }
        };

        const paginationOptions: Pagination = paginationFactory({
            sizePerPage: 25
        });

        return(
            <div>
                <BootstrapTable 
                    keyField='id' 
                    selectRow={ this.props.options && this.props.options.deleteRow ? selectRow : undefined }
                    data={ this.props.data.filter((a: any) => this.state.deletedIds && !this.state.deletedIds.includes(a.id)) } 
                    columns={ this.props.columns } 
                    striped={ true } 
                    hover={ true }
                    noDataIndication = { () => <h2>No data records found</h2> }
                    cellEdit={ cellEdit } 
                    pagination={  this.props.options && this.props.options.pagination === true ? paginationOptions : undefined }
                />
                <button onClick={() => this.remove()} className="btn btn-danger">Delete</button>
            </div>
        );
    }
    
    private onAfterSaveCell = (oldValue: string, newValue: string, row: { [x: string]: string; }, column: { [x: string]: string; }) => {
        const field = 'id';
        const dataField = 'dataField';
        const id = Number(row[field]);
        this.update(column[dataField], newValue, id)
    }
      
    private onBeforeSaveCell(row: any, cellName: any, cellValue: any) {
        // You can do any validation on here for editing value,
        // return false for reject the editing
        return true;
    }

    private update = (key: string, value: any, id: number) => {
        this.setState({ ...this.state, ...{ loading: true }}) 

        const updateModel: IUpdateRequest = {
            table: this.props.table, 
            field: key, 
            value: value, 
            id: id
        }

        commonApi.update(updateModel);
    }

    private remove = () => {
        if (this.state.selectedId && this.props.options && this.props.options.deleteRow) {
            commonApi.remove(this.state.selectedId, this.props.table);

            this.setState(prevState => ({ 
                deletedIds: [...prevState.deletedIds, this.state.selectedId] 
            })) 
        }
    }
}