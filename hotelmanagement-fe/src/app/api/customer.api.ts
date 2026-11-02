import { fetchApi } from "./client";
import type { CustomerResponseDto } from "@/types/customer";

export const getCustomers = (): Promise<CustomerResponseDto[]> =>
    fetchApi<CustomerResponseDto[]>("/Customer");

export const getCustomerById = (customerId: number): Promise<CustomerResponseDto> =>
    fetchApi<CustomerResponseDto>(`/Customer/${customerId}`);