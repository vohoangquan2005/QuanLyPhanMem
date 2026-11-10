import { fetchApi } from "./client";
import type { InvoiceResponseDto } from "@/types/invoice";

export const getInvoices = (): Promise<InvoiceResponseDto[]> =>
    fetchApi<InvoiceResponseDto[]>("/Invoice");

export const getMyInvoices = (): Promise<InvoiceResponseDto[]> =>
    fetchApi<InvoiceResponseDto[]>("/Invoice/my-invoices");

export const getMyInvoice = (id: number): Promise<InvoiceResponseDto> =>
    fetchApi<InvoiceResponseDto>(`/Invoice/my-invoices/${id}`);

export const deleteInvoice = (id: number): Promise<void> =>
    fetchApi<void>(`/Invoice/${id}`, { method: "DELETE" });