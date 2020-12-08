import * as React from "react";
import { api } from 'src/Api/Api';
import { SelectionRefinement } from 'src/components/SelectionRefinement/SelectionRefinement';
import { IGoogleAutoCompleteSearch, IPrediction } from 'src/models/IGoogleAutoComplete';


export interface IOwnProps {
    onStoreChange: (customer: IGoogleAutoCompleteSearch) => void
}

export interface IOwnState {
    filter: string,
    stores: IGoogleAutoCompleteSearch[]
    loading: boolean
}

export default class PickupLocation extends React.Component<IOwnProps, IOwnState> {

    constructor(props: any) {
        super(props);
        this.state = {
            filter: "",
            stores: [],
            loading: false
        }
    }

    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (prevState.filter !== this.state.filter && this.state.filter !== "") {
            api.googleAutoComplete(this.state.filter)
                .then((c) => this.autoCompleteSuccess(c.predictions));
        }
    }

    public render(): JSX.Element {
        return (
            <div>
                <SelectionRefinement<IGoogleAutoCompleteSearch>
                    label="Pickup Location"
                    placeholder="Search local stores..."
                    filter={this.state.filter} 
                    onChange={(f) => this.pickupLocationChanged(f)} 
                    setFilterToItemName={true}
                    itemSelected={(i) => this.selectedStore(i)}
                    loading={this.state.loading}
                    filteredResults={this.state.stores} />
            </div>
                    
        );
    }

    private selectedStore = (store: IGoogleAutoCompleteSearch) => {
        this.props.onStoreChange(store);
    }

    private pickupLocationChanged = (filter: string) => {
        this.setState({ 
            loading: true,
            filter: filter 
        })
    }

    private autoCompleteSuccess = (stores: IPrediction[]) => {
        this.setState({ ...this.state,
            ...{ 
                loading: false,
                stores: stores.map(store =>({ 
                    id: store.place_id,
                    name: store.description
                } as IGoogleAutoCompleteSearch))
            }
        }) 
      }

}