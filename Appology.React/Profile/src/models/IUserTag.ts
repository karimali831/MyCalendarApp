import { DayOfWeek } from "src/Enums/DayOfWeek";
import { TimeFrequency } from "src/Enums/TimeFrequency";

export interface IUserTag {
    id: string,
    typeID: number,
    name: string,
    themeColor: string,
    targetFrequency?: TimeFrequency
    targetValue?: number,
    targetUnit: string,
    startDayOfWeek: DayOfWeek,
    endDayOfWeek: DayOfWeek
}