import IStoreState from '../../../state/IStoreState';

export const getHash = (state: IStoreState): string =>
    state.router.location.hash;

export const getLocation = (state: IStoreState): string =>
    state.router.location.pathname;

export const getSearch = (state: IStoreState): string =>
    state.router.location.search;