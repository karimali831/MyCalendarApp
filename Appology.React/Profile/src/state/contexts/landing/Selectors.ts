import IStoreState from "src/state/IStoreState";

export const getFilter = (state: IStoreState): string => {
    return state.landing.filter
}

