import { fetchApi } from "./client";
import type {
    OccupancyDailyStatisticsDto,
    RevenueDailyStatisticsDto,
} from "@/types/statistics";

function rangeQuery(startDate: string, endDate: string) {
    const query = new URLSearchParams({ startDate, endDate });
    return query.toString();
}

export const getRevenueByDateRange = (startDate: string, endDate: string): Promise<RevenueDailyStatisticsDto[]> =>
    fetchApi<RevenueDailyStatisticsDto[]>(`/Statistics/revenue/date-range?${rangeQuery(startDate, endDate)}`);

export const getOccupancyByDateRange = (startDate: string, endDate: string): Promise<OccupancyDailyStatisticsDto[]> =>
    fetchApi<OccupancyDailyStatisticsDto[]>(`/Statistics/occupancy/date-range?${rangeQuery(startDate, endDate)}`);