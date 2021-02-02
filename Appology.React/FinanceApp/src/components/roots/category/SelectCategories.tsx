import * as React from 'react';
import { Load } from 'src/components/base/Loader';
import { OnChangeSelectedCategoryAction, OnChangeSelectedSecondCategoryAction, LoadCategoriesAction } from 'src/state/contexts/common/Actions';
import { CategoryType } from 'src/enums/CategoryType';
import { ICategory } from 'src/models/ICategory';

export interface IOwnProps {
    categories: ICategory[],
    secondCategories: ICategory[],
    loadingCategories: boolean,
    loadingSecondCategories: boolean,
    categoryType?: CategoryType,
    secondTypeId?: number | undefined,
    selectedCat?: number | undefined,
    selectedSecondCat?: number  | undefined,
    showAllSubcats?: boolean | false,
    loadCategories: (categoryType: CategoryType) => LoadCategoriesAction
    onChangeSelectedCategory: (selectedCat: number, secondTypeId?: number) => OnChangeSelectedCategoryAction,
    onChangeSelectedSecondCategory: (selectedSecondCat: number) => OnChangeSelectedSecondCategoryAction
}

export interface IOwnState{
    selectedCat?: number | undefined,
    secondTypeId?: number | undefined,
    selectedSecondCat?: number  | undefined
}

export default class SelectCategories extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            selectedCat: this.props.selectedCat,
            secondTypeId: this.props.secondTypeId,
            selectedSecondCat: this.props.selectedSecondCat
        };
    }

    public componentDidMount(){
        if (this.props.categoryType !== undefined) {
            this.props.loadCategories(this.props.categoryType);
        }
    }

    public componentDidUpdate(prevProps: IOwnProps, prevState: IOwnState) {
        if (this.state.selectedCat !== undefined) {
            if (prevState.secondTypeId !== this.state.secondTypeId || prevState.selectedCat !== this.state.selectedCat) {
                this.props.onChangeSelectedCategory(this.state.selectedCat, this.state.secondTypeId);
            }

            if (prevState.selectedSecondCat !== this.state.selectedSecondCat && this.state.selectedSecondCat !== undefined) {
                this.props.onChangeSelectedSecondCategory(this.state.selectedSecondCat);
            }
        }
    }
    
    public render() {
        return (
            <div>
                {
                    this.props.loadingCategories ? <Load /> :
                    <>
                        <select id="categories" onChange={e => this.onChangeSelectedCategory(e)} className="form-control">
                            <option value={0}>-- select category --</option>
                            {
                                this.props.categories.map(c =>
                                    <option key={c.id} value={c.id + "-" + c.secondTypeId} disabled={c.disabled} selected={this.state.selectedCat === c.id}>{c.name}</option>
                                )
                            }
                        </select>
                    </>
                }
                <>
                    {
                        this.state.secondTypeId !== undefined ?
                            this.props.loadingSecondCategories ? <Load /> :
                            <>
                                <select id="categories" onChange={e => this.onChangeSelectedSecondCategory(e)} className="form-control">
                                    <option value={0}>-- select subcategory --</option>
                                    {
                                        this.props.showAllSubcats 
                                            ? <option value={9999}>All</option>  
                                            : null
                                    }
                                    {
                                        this.props.secondCategories.map(c =>
                                            <option key={c.id} value={c.id} disabled={c.disabled} selected={this.state.selectedSecondCat === c.id}>{c.name}</option>
                                        )
                                    }
                                </select>
                            </>
                        : null
                    }
                </>
            </div>
        )
    }

    private onChangeSelectedCategory = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const value = e.target.value;
        const ids = value.split("-", 2);

        this.setState({ ...this.state, 
            ...{ 
                selectedCat: Number(ids[0]),
                secondTypeId: ids[1] !== "null" && ids[0] !== "0" && ids[1] !== "0" ? Number(ids[1]) : undefined
            }
        })  
    }

    private onChangeSelectedSecondCategory = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const cat = e.target.value;

        this.setState({ ...this.state, 
            ...{ 
                selectedSecondCat: Number(cat)
            }
        })  
    }
}