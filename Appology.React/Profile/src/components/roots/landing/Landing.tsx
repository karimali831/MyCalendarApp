import { Variant } from '@appology/react-components/src/Enums/Variant';
import IBaseModel from '@appology/react-components/src/SelectionRefinement/IBaseModel';
import * as React from 'react';
import { api, IDummyResponse } from 'src/Api/Api';
import { showAlert } from 'src/components/utils/Utils';

export interface IPropsFromDispatch {
}

export interface IPropsFromState {
    dummy: IBaseModel[],
    filter: string
}

export interface IOwnState {
    loading: boolean
}

type AllProps = IPropsFromState & IPropsFromDispatch;

export default class Landing extends React.Component<AllProps, IOwnState> {

    constructor(props: AllProps) {
        super(props);
        this.state = {
            loading: true
        };
    }

    public componentDidMount() {
        this.getDummy();
    }

    public componentDidUpdate = (prevProps: AllProps, prevState: IOwnState) => {
        if (this.props.filter !== prevProps.filter) {
            this.getDummy();
        }
    }

    public render() {

        return (
            <div>
                <h1>Hello!</h1>
            </div>
        );
    }    


    private getDummy() {
        this.setState({ loading: true })

        api.dummy(this.props.filter)
            .then(response => this.dummySuccess(response))

    }


    private dummySuccess = (dummy: IDummyResponse) => {
        this.setState({ loading: false })

        if (dummy.status) {
            showAlert("Data loaded successfully!")
        }
        else
        {
            showAlert("Unable to load order", Variant.Danger);
        }
    }
}