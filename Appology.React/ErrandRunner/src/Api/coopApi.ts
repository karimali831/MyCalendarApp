export class CoopApi {

    public searchItems = async (apiUrl: string, request: ISearchItemsRequests): Promise<ISearchItemsResponse> => {
        return fetch(apiUrl, {
            method: "POST",
            body: JSON.stringify(request),
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: 'same-origin',
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.json();

        })
        .then(data => data as ISearchItemsResponse);
    }
}

export const coopApi = new CoopApi();

export interface ISearchItemsRequests {
    requests: ISearchItemsRequest[]
}

export interface ISearchItemsRequest {
    indexName: string,
    query: string,
    filters: string,
    hitsPerPage: number,
    page: number,
    userToken: string,
    clickAnalytics: boolean,
    facets: [],
    tagFilters: string
}

export interface ISearchItemsResponse {
    results: ISearchItemResults[]
}

export interface ISearchItemResults {
    hits: IItemHits[]
}

export interface IItemHits {
    id: string,
    name: string,
    images: IItemHitsImages[],
    price: number,
    maxQuantity: number
}

export interface IItemHitsImages {
    mediaDimensionWidth: number,
    mediaStorageKey: string
}
