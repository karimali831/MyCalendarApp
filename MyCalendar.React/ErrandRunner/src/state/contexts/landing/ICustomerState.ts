import { ICustomer } from 'src/models/ICustomer';

export default interface ICustomerState {
    customerFilter: string | undefined,
    customers: ICustomer[]
}

export class CustomerState {
    public static readonly intialState = {
        customerFilter: undefined,
        customers: []
    }
}