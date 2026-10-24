
"use client";

import React, { useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
    LayoutDashboard,
    Users,
    BedDouble,
    CalendarDays,
    ConciergeBell,
    FileClock,
    BarChart3,
    CreditCard,
    Receipt,
    ChevronLeft,
    ChevronRight,
    LogOut,
    Hotel,
    Shield,
    UserRound,
    Gift,
    Star,
    XCircle,
    ScrollText,
    Image,
    Package,
} from "lucide-react";
import { useAuth } from "@/contexts/AuthContext";
import { Button } from "@/components/ui/button";

export interface NavItem {
    title: string;
    href: string;
    icon: React.ComponentType<{ className?: string }>;
}

const adminNavItems: NavItem[] = [
    {
        title: "Tổng quan",
        href: "/admin/dashboard",
        icon: LayoutDashboard,
    },
    {
        title: "Vai trò",
        href: "/admin/roles",
        icon: Shield,
    },
    {
        title: "Tài khoản",
        href: "/admin/users",
        icon: Users,
    },
    {
        title: "Khách hàng",
        href: "/admin/customer",
        icon: Users,
    },
    {
        title: "Khách lưu trú",
        href: "/admin/guests",
        icon: UserRound,
    },
    {
        title: "Loại phòng",
        href: "/admin/room-types",
        icon: BedDouble,
    },
    {
        title: "Danh mục phòng",
        href: "/admin/rooms",
        icon: BedDouble,
    },
    {
        title: "Ảnh phòng",
        href: "/admin/room-images",
        icon: Image,
    },
    {
        title: "Khuyến mãi",
        href: "/admin/promotions",
        icon: Gift,
    },
    {
        title: "Dịch vụ",
        href: "/admin/services",
        icon: ConciergeBell,
    },
    {
        title: "Gói dịch vụ",
        href: "/admin/service-packages",
        icon: Package,
    },
    {
        title: "Chính sách hủy",
        href: "/admin/cancellation-policies",
        icon: FileClock,
    },
    {
        title: "Quản lý đặt phòng",
        href: "/admin/bookings",
        icon: CalendarDays,
    },
    {
        title: "Dịch vụ đã đặt",
        href: "/admin/booking-services",
        icon: ConciergeBell,
    },
    {
        title: "Yêu cầu hủy",
        href: "/admin/cancellations",
        icon: XCircle,
    },
    {
        title: "Hóa đơn",
        href: "/admin/invoices",
        icon: Receipt,
    },
    {
        title: "Thanh toán",
        href: "/admin/payments",
        icon: CreditCard,
    },
    {
        title: "Đánh giá",
        href: "/admin/reviews",
        icon: Star,
    },
    {
        title: "Báo cáo thống kê",
        href: "/admin/statistics",
        icon: BarChart3,
    },
    {
        title: "Nhật ký thao tác",
        href: "/admin/audit-logs",
        icon: ScrollText,
    },
];

interface AdminSidebarProps {
    onItemClick?: () => void;
}

export default function AdminSidebar({ onItemClick }: AdminSidebarProps) {
    const pathname = usePathname();
    const { user, logout } = useAuth();
    const [collapsed, setCollapsed] = useState(false);

    return (
        <aside
            className={`sticky top-0 flex h-screen shrink-0 flex-col border-r border-sidebar-border bg-sidebar text-sidebar-foreground transition-all duration-300 ${
                collapsed ? "w-20" : "w-64"
            }`}
        >
            {/* Header của Sidebar */}
            <div className="flex h-20 items-center justify-between border-b border-sidebar-border px-4">
                <Link
                    href="/"
                    className={`flex items-center gap-2.5 font-bold overflow-hidden transition-all ${
                        collapsed ? "justify-center w-full" : ""
                    }`}
                    title="HellStay Admin"
                >
                    <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-sidebar-primary text-sidebar-primary-foreground shadow-sm">
                        <Hotel className="size-5" />
                    </div>

                    {!collapsed && (
                        <div className="flex flex-col">
                            <span className="text-base font-bold tracking-tight text-sidebar-foreground">
                                HellStay
                            </span>
                            <span className="text-[10px] font-semibold text-sidebar-primary uppercase tracking-widest">
                                Quản trị viên
                            </span>
                        </div>
                    )}
                </Link>
            </div>

            {/* Nút thu gọn / mở rộng (Desktop) */}
            <button
                onClick={() => setCollapsed(!collapsed)}
                className="absolute -right-3 top-24 z-10 hidden md:flex size-6 items-center justify-center rounded-full border border-sidebar-border bg-sidebar text-sidebar-foreground shadow-sm transition hover:bg-sidebar-accent"
                aria-label={
                    collapsed ? "Mở rộng sidebar" : "Thu gọn sidebar"
                }
            >
                {collapsed ? (
                    <ChevronRight className="size-3.5" />
                ) : (
                    <ChevronLeft className="size-3.5" />
                )}
            </button>

            {/* Menu danh sách chức năng */}
            <div className="flex-1 overflow-y-auto py-4 px-3">
                <div className="mb-2 px-2">
                    {!collapsed && (
                        <span className="text-[10px] font-semibold uppercase tracking-wider text-sidebar-foreground/55">
                            Chức năng quản trị
                        </span>
                    )}
                </div>

                <nav className="space-y-1">
                    {adminNavItems.map((item) => {
                        const Icon = item.icon;
                        const isActive =
                            pathname === item.href ||
                            (item.href !== "/admin" &&
                                pathname.startsWith(item.href));

                        return (
                            <Link
                                key={item.href}
                                href={item.href}
                                onClick={onItemClick}
                                title={collapsed ? item.title : undefined}
                                className={`group flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors ${
                                    isActive
                                    ? "bg-sidebar-primary text-sidebar-primary-foreground shadow-sm font-semibold"
                                        : "text-sidebar-foreground/75 hover:bg-sidebar-accent hover:text-sidebar-foreground"
                                } ${collapsed ? "justify-center px-2" : ""}`}
                            >
                                <Icon
                                    className={`size-5 shrink-0 transition-transform ${
                                        isActive
                                            ? "scale-105"
                                            : "group-hover:scale-105"
                                    }`}
                                />

                                {!collapsed && (
                                    <span className="truncate">
                                        {item.title}
                                    </span>
                                )}
                            </Link>
                        );
                    })}
                </nav>
            </div>

            {/* Phần chân Sidebar: Thông tin người dùng & Đăng xuất */}
            <div className="border-t border-sidebar-border p-3">
                <div
                    className={`flex items-center gap-3 rounded-xl bg-sidebar-accent/70 p-2.5 ${
                        collapsed ? "justify-center" : ""
                    }`}
                >
                    <div className="flex size-8 shrink-0 items-center justify-center rounded-full bg-sidebar-primary text-sidebar-primary-foreground font-bold text-xs">
                        {user?.fullName?.charAt(0).toUpperCase() || "A"}
                    </div>

                    {!collapsed && (
                        <div className="min-w-0 flex-1">
                            <p className="truncate text-xs font-semibold text-sidebar-foreground">
                                {user?.fullName || "Admin"}
                            </p>
                            <p className="truncate text-[10px] text-sidebar-foreground/60">
                                {user?.email || ""}
                            </p>
                        </div>
                    )}

                    {!collapsed && (
                        <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => logout()}
                            title="Đăng xuất"
                            className="size-7 text-sidebar-foreground/70 hover:text-white hover:bg-destructive/80"
                        >
                            <LogOut className="size-4" />
                        </Button>
                    )}
                </div>
            </div>
        </aside>
    );
}
