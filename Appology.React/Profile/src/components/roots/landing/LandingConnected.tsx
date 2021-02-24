import IStoreState from '../../../state/IStoreState';
import { connect } from 'react-redux';
import Landing, { IPropsFromState, IPropsFromDispatch } from './Landing';
import {  LoadUser } from 'src/state/contexts/profile/Action';

// REACT-REDUX
// Wrap stateless component with redux connected component

// Map full state to state required for component
const mapStateToProps =
    (state: IStoreState): IPropsFromState => ({
        user: state.profile.user,
        groups: state.profile.groups,
        loading: state.profile.loading
    });

// Add required action creators for component
const mapPropsFromDispatch: IPropsFromDispatch =
{
    getUser: LoadUser.creator
};

// This does the magic of subscribing to state changes and ensuring the wrapped
// stateless component gets all the properties it needs from the Redux state
export default connect(mapStateToProps, mapPropsFromDispatch)(Landing);
