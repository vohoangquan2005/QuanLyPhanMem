import { fetchApi } from "./client";
import type { ServiceResponseDto } from "@/types/service";

export const getServices = () =>
    fetchApi<ServiceResponseDto[]>("/Service");

export const getServiceById = (id: number) =>
    fetchApi<ServiceResponseDto>(`/Service/${id}`);

export const getDeletedServices = () =>
    fetchApi<ServiceResponseDto[]>("/Service/deleted");

export const createService = (data: ServiceWriteDto) =>
    fetchApi<ServiceResponseDto>("/Service", {
        method: "POST",
        body: JSON.stringify(data),
    });

export const updateService = (id: number, data: ServiceWriteDto) =>
    fetchApi<void>(`/Service/${id}`, {
        method: "PUT",
        body: JSON.stringify(data),
    });

export const softDeleteService = (id: number) =>
    fetchApi<void>(`/Service/${id}/soft-delete`, {
        method: "PATCH",
    });

export const restoreService = (id: number) =>
    fetchApi<void>(`/Service/${id}/restore`, {
        method: "PATCH",
    });

export const deleteService = (id: number) =>
    fetchApi<void>(`/Service/${id}`, {
        method: "DELETE",
    });

export interface ServiceWriteDto {
    serviceName: string;
    unitPrice: number;
    unit: string;
    description: string | null;
}