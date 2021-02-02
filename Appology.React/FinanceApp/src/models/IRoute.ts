export interface IRoute {
    id: number,
    weekNo: string,
    routeType: string
    routeDate: Date,
    mileage: number,
    extaMileage: number,
    mpg: number,
    extraDrops: number,
    info: string,
    fuelCost: number,
    coFuel: number
}

export interface IRouteDTO {
    routeTypeId: number,
    routeDate: string,
    mileage: number | undefined,
    extraMileage: number | undefined,
    mpg: number | undefined,
    extraDrops: number | undefined,
    info: string,
    fuelCost: number | undefined,
    coFuel: number | undefined
}
