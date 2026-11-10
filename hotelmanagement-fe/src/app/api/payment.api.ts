import { fetchApi } from "./client";
import type { PaymentResponseDto } from "@/types/payment";

export const getPayments = () =>
    fetchApi<PaymentResponseDto[]>("/Payment");

export const getMyPayments = () =>
    fetchApi<PaymentResponseDto[]>("/Payment/my-payments");

export const submitPayment = (
    id: number,
    data: {
        paymentMethod: string;
        transactionCode?: string;
    }
) =>
    fetchApi<PaymentResponseDto>(`/Payment/${id}/submit`, {
        method: "PUT",
        body: JSON.stringify(data),
    });

export const requestInvoiceBalancePayment = (invoiceId: number) =>
    fetchApi<PaymentResponseDto>(`/Payment/my-invoices/${invoiceId}/request`, {
        method: "POST",
    });

export const confirmPayment = (id: number) =>
    fetchApi<void>(`/Payment/${id}/confirm`, {
        method: "PUT",
    });

export const rejectPayment = (id: number) =>
    fetchApi<void>(`/Payment/${id}/reject`, {
        method: "PUT",
    });

export const deletePayment = (id: number) =>
    fetchApi<void>(`/Payment/${id}`, {
        method: "DELETE",
    });