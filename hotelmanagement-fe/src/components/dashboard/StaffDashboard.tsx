"use client";

import { useEffect, useState } from "react";
import {
    BedDouble,
    CalendarCheck,
    CircleAlert,
    LoaderCircle,
    RefreshCw,
    Users,
} from "lucide-react";

import {
    Card,
    CardContent,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";

import { getDashboardOverview } from "@/app/api/dashboard.api";
import type { DashboardResponseDto } from "@/types/dashboard";

export default function StaffDashboard() {
    const [data, setData] = useState<DashboardResponseDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    async function load() {
        setLoading(true);
        setError("");

        try {
            setData(await getDashboardOverview());
        } catch (cause) {
            setError(
                cause instanceof Error
                    ? cause.message
                    : "Không thể tải dashboard."
            );
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        let active = true;

        void getDashboardOverview()
            .then((overview) => {
                if (active) setData(overview);
            })
            .catch((cause: unknown) => {
                if (active) {
                    setError(
                        cause instanceof Error
                            ? cause.message
                            : "Không thể tải dashboard."
                    );
                }
            })
            .finally(() => {
                if (active) setLoading(false);
            });

        return () => {
            active = false;
        };
    }, []);

    const metrics = data
        ? [
            {
                title: "Phòng trống",
                value: data.availableRooms,
                detail: `${data.occupiedRooms} đang có khách · ${data.maintenanceRooms} bảo trì`,
                icon: BedDouble,
            },
            {
                title: "Nhận phòng hôm nay",
                value: data.todayCheckIns,
                detail: "Theo ngày dự kiến trong hệ thống",
                icon: CalendarCheck,
            },
            {
                title: "Trả phòng hôm nay",
                value: data.todayCheckOuts,
                detail: "Theo ngày dự kiến trong hệ thống",
                icon: CalendarCheck,
            },
            {
                title: "Booking chờ xử lý",
                value: data.pendingBookings,
                detail: `${data.confirmedBookings} đã xác nhận`,
                icon: Users,
            },
        ]
        : [];

    return (
        <div className="space-y-7">
            <div>
                <p className="text-xs font-semibold uppercase tracking-[0.16em] text-primary">
                    Nhân viên khách sạn
                </p>
                <h1 className="mt-2 text-3xl font-bold tracking-tight sm:text-4xl">
                    Tổng quan ca trực
                </h1>
                <p className="mt-2 text-sm text-muted-foreground">
                    Các đầu việc vận hành cần theo dõi trong ngày.
                    </p>
            </div>

            {loading ? (
                <div className="flex min-h-64 items-center justify-center gap-3 rounded-2xl border bg-card text-sm text-muted-foreground shadow-sm">
                    <LoaderCircle className="size-5 animate-spin" />
                    Đang tải số liệu…
                </div>
            ) : error ? (
                <div
                    role="alert"
                    className="rounded-2xl border border-destructive/30 bg-card p-6 text-sm text-destructive shadow-sm"
                >
                    <div className="flex items-center gap-2">
                        <CircleAlert className="size-5" />
                        {error}
                    </div>

                    <button
                        onClick={() => void load()}
                        className="mt-4 inline-flex items-center gap-2 rounded-md border px-3 py-2 text-foreground"
                    >
                        <RefreshCw className="size-4" />
                        Thử lại
                    </button>
                </div>
            ) : !data ? (
                <div className="rounded-2xl border border-dashed bg-card p-10 text-center text-sm text-muted-foreground">
                    Không tải được dữ liệu.
                </div>
            ) : (
                <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
                    {metrics.map(({ title, value, detail, icon: Icon }) => (
                            <Card key={title} className="dashboard-metric-card min-h-40">
                                <CardHeader className="flex flex-row items-center justify-between pb-1">
                                    <CardTitle className="text-sm font-medium text-muted-foreground">
                                    {title}
                                </CardTitle>

                                    <span className="flex size-10 items-center justify-center rounded-xl bg-primary/10 text-primary">
                                        <Icon className="size-5" />
                                    </span>
                            </CardHeader>

                            <CardContent>
                                <p className="text-3xl font-bold tracking-tight">
                                    {value.toLocaleString("vi-VN")}
                                </p>

                                <p className="mt-2 text-xs text-muted-foreground">
                                    {detail}
                                </p>
                            </CardContent>
                        </Card>
                    ))}
                </div>
            )}
        </div>
    );
}
