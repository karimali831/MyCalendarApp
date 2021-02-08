import * as React from 'react';
import { commonApi } from '../../../Api/CommonApi'
import { Redirect } from 'react-router-dom'
import { Load } from '@appology/react-components'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faArrowRight } from '@fortawesome/free-solid-svg-icons';
import { ICategory, ICategoryDTO } from '../../../models/ICategory';
import { appPathUrl } from 'src/components/utils/Utils';

interface IOwnProps {
}

export interface IOwnState {
    loading: boolean,
    redirect: boolean,
    catName: string,
    selectedSecondTypeId: number | null,
    catsWithSubs: ICategory[],
    addSubCat: boolean,
}

export default class AddCategory extends React.Component<IOwnProps, IOwnState> {
    constructor(props: IOwnProps) {
        super(props);
        this.state = { 
            loading: false,
            redirect: false,
            catName: "",
            selectedSecondTypeId: null,
            catsWithSubs: [],
            addSubCat: false
        };
    }


    public componentDidUpdate(prevProps: IOwnProps, prevState: IOwnState) {
        if (prevState.addSubCat !== this.state.addSubCat) {
            this.loadCategoriesWithSubs()
        }
    }

    public render() {
        const { redirect, loading } = this.state;

        if (redirect) {
            return <Redirect to={`${appPathUrl}/category`} />;
        }

        if (loading) {
            return <Load />
        }

        return (
            <div>
                <button type="button" onClick={() => this.addSubCat()} className="btn btn-link">
                    <FontAwesomeIcon icon={faArrowRight} /> Add {this.catTxt(this.state.addSubCat)}
                </button>
                <form className="form-horizontal">
                    <div className="form-group form-group-lg">
                        <input className="form-control" type="text" value={this.state.catName} placeholder={"Enter " + this.catTxt(!this.state.addSubCat)} onChange={(e) => { this.onCatInputChanged(e);}} />
                    </div>
                    {
                        this.state.addSubCat ?
                        <>
                            <div className="form-group form-group-lg">
                                <select onChange={e => this.onChangeSelectedCategory(e)} className="form-control">
                                    <option value={0}>-- select category --</option>
                                    {
                                        this.state.catsWithSubs.map(c =>
                                            <option key={c.id} value={c.secondTypeId}>{c.name}</option>
                                        )
                                    }
                                </select>
                            </div>
                        </> 
                        : null
                    }
                    <button className="btn btn-primary" onClick={() =>this.addCategory() }>{"Add " + this.catTxt(!this.state.addSubCat)}</button>
                </form>
            </div>
        )
    }

    private loadCategoriesWithSubs = () => {
        commonApi.categories(undefined, this.state.addSubCat)
            .then(response => this.loadCategoriesSuccess(response.categories));
    }

    private loadCategoriesSuccess = (categories: ICategory[]) => {
        this.setState({ ...this.state,
            ...{ 
                loading: false, 
                catsWithSubs: categories
            }
        }) 
    }

    private addSubCat = () => {
        this.setState({ ...this.state, addSubCat: !this.state.addSubCat })
    }

    private catTxt = (addSubCat: boolean) => addSubCat ? "Category" : "Sub-Category"; 
    
    private onCatInputChanged = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.setState({ ...this.state, catName: e.target.value })
    }

    private onChangeSelectedCategory = (e: React.ChangeEvent<HTMLSelectElement>) => {
        this.setState({ ...this.state, selectedSecondTypeId: Number(e.target.value) })
    }

    private addCategory = () => {
        if (this.state.catName && this.state.catName.length > 2)
        {
            const addModel: ICategoryDTO = {
                name: this.state.catName,
                typeId: this.state.selectedSecondTypeId
            }

            commonApi.add(addModel, "categories");
            this.setState({ ...this.state, redirect: true })  
        }
        else{
            alert("Enter category name");
        }
    }
}