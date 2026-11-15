import { fetchApi } from "./client";
import type { DashboardResponseDto } from "@/types/dashboard";

export const getDashboardOverview = () =>
    fetchApi<DashboardResponseDto>("/Dashboard");