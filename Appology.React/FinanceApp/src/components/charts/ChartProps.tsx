import * as React from 'react'
import { ChartType, ChartDataType } from 'src/enums/ChartType';
import { IChartModel, IChartSummary } from '../../models/IChart';
import { Chart } from './Chart';
import { IDateFilter } from 'src/models/IDateFilter';
import { DataType } from 'src/enums/DataType';
import { IMonthComparisonChartRequest } from 'src/Api/Api';
import { CategoryType } from 'src/enums/CategoryType';

export interface IPropsFromState {
	loading: boolean,
	headerText?: string | null,
	dataLabels: boolean,
	chartSummary?: IChartSummary[] | null,
	chart: IChartModel,
	chartType: ChartType,
	width: number,
	height: number,
	dateFilter: IDateFilter,
	categoryType?: CategoryType,
	dataType: DataType,
	chartDataType: ChartDataType,
	secondTypeId?: number,
	selectedCat?: number,
	selectedSecondCat?: number,
	maxCats?: number | null,
	request?: IMonthComparisonChartRequest
}

export interface IPropsFromDispatch {
	dateFilterChanged: (filter: IDateFilter, dataType: DataType) => void,
	chartChanged?: (request: IMonthComparisonChartRequest, type: ChartDataType) => void,
	maxCatsChanged?: (maxCats: number) => void
}

type AllProps = IPropsFromState & IPropsFromDispatch

const ChartProps: React.SFC<AllProps> = (props) =>
	<Chart
		loading={props.loading}
		headerText={props.headerText}
		dataLabels={props.dataLabels}
		chartSummary={props.chartSummary}
		chartType={props.chartType}
		chart={props.chart}
		width={props.width}
		height={props.height} 
		dateFilter={props.dateFilter}
		categoryType={props.categoryType}
		dataType={props.dataType}
		chartDataType={props.chartDataType}
		secondTypeId={props.secondTypeId}
		selectedCat={props.selectedCat}
		selectedSecondCat={props.selectedSecondCat}
		chartChanged={props.chartChanged}
		dateFilterChanged={props.dateFilterChanged}
		maxCatsChanged={props.maxCatsChanged}
		maxCats={props.maxCats}
		request={props.request} />

export default ChartProps;
