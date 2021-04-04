import { SelectionRefinement } from '@appology/react-components';
import * as React from 'react';
import { coopApi, ISearchItemsRequests, ISearchItemsResponse } from 'src/Api/coopApi';
import { IPlace, IPlaceItemSearch } from 'src/models/IPlace';

export interface IOwnState {
    apiItems: IPlaceItemSearch[],
    loading: boolean
}

export interface IOwnProps {
    place: IPlace,
    itemSelected: (item: IPlaceItemSearch) => void
}

export class OrderItemsSearch extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            apiItems: [],
            loading: false
        };
    }

    public render() {
        return (
            <SelectionRefinement<IPlaceItemSearch>
                label=""
                placeholder="Search for groceries" 
                onChange={this.searchItems} 
                loading={this.state.loading}
                setFilterToItemName={false}
                itemSelected={this.props.itemSelected}
                filteredResults={this.state.apiItems} 
            />
        );
    }

    private searchItems = (value: string) => {
        this.setState({ loading: true })

        // co op
        if (this.props.place.placeId === "ChIJb6Ww1T-n2EcRtcyDimKZ9F0") {
            const request : ISearchItemsRequests = {
                requests: [
                    {
                        indexName: "production",
                        query: value,
                        filters: "stores.id:17eda196-0394-4cf5-9053-a7652fc76671",
                        hitsPerPage: 5,
                        page: 0,
                        userToken: "04edba39-0b3b-461e-8522-359f738ed048",
                        clickAnalytics: true,
                        facets: [],
                        tagFilters: ""
                    }
                ]
            }

            coopApi.searchItems(this.props.place.apiProductUrl, request)
                .then(response => this.searchItemsSuccess(response))
        }
    }

    private searchItemsSuccess = (response: ISearchItemsResponse) => {
        this.setState({ loading: false })

        if (response.results.length > 0) {
            this.setState({
                apiItems: response.results[0].hits.map(item =>({ 
                    id: item.id, 
                    name: item.name,
                    leftImage: item.images.find(x => x.mediaDimensionWidth === 100)?.mediaStorageKey,
                    rightContent: <>£{item.price}</>,
                    price: item.price,
                    maxQuantity: item.maxQuantity
                } as IPlaceItemSearch))
            })
        }
    }
}