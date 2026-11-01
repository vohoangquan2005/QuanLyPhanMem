"use client";

import { useEffect, useMemo, useState, type FormEvent } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { ArrowLeft, CheckCircle2, CircleAlert, LoaderCircle } from "lucide-react";

import { createBooking } from "@/app/api/booking.api";
import { getCustomers } from "@/app/api/customer.api";
import { getAll } from "@/app/api/room.api";
import { getServicePackages } from "@/app/api/service-package.api";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useAuth } from "@/contexts/AuthContext";
import type { BookingResponseDto } from "@/types/booking";
import type { CustomerResponseDto } from "@/types/customer";
import type { RoomResponseDto } from "@/types/room";
import type { ServicePackageDto } from "@/types/service-package";

function todayInput() {
    const date = new Date();
    date.setMinutes(date.getMinutes() - date.getTimezoneOffset());
    return date.toISOString().slice(0, 16);
}

const money = (amount: number) => `${amount.toLocaleString("vi-VN")} VNĐ`;

export default function StaffBookingForm() {
    const router = useRouter();
    const { user, isLoading: authLoading } = useAuth();

    const [customers, setCustomers] = useState<CustomerResponseDto[]>([]);
    const [rooms, setRooms] = useState<RoomResponseDto[]>([]);
    const [packages, setPackages] = useState<ServicePackageDto[]>([]);
    const [customerId, setCustomerId] = useState("");
    const [selectedRoomIds, setSelectedRoomIds] = useState<number[]>([]);
    const [roomGuests, setRoomGuests] = useState<Record<number, number>>({});
    const [servicePackageId, setServicePackageId] = useState("");
    const [checkIn, setCheckIn] = useState("");
    const [checkOut, setCheckOut] = useState("");
    const [note, setNote] = useState("");
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState("");
    const [packageError, setPackageError] = useState("");
    const [created, setCreated] = useState<BookingResponseDto | null>(null);

    useEffect(() => {
        if (authLoading) return;

        if (!user) {
            router.replace("/login?redirect=%2Fstaff%2Fbookings%2Fnew");
            return;
        }

        if (user.roleName !== "Staff" && user.roleName !== "Admin") {
            router.replace("/");
            return;
        }

        let active = true;

        void Promise.all([
            getCustomers(),
            getAll(),
            getServicePackages().catch((cause: unknown) => {
                if (active) {
                    setPackageError(
                        cause instanceof Error
                            ? cause.message
                            : "Không thể tải gói dịch vụ.",
                    );
                }

                return [] as ServicePackageDto[];
            }),
        ])
            .then(([customerResult, roomResult, packageResult]) => {
                if (!active) return;

                setCustomers(customerResult);
                setRooms(roomResult.filter((room) => room.isDeleted === 0));
                setPackages(packageResult);
            })
            .catch((cause: unknown) => {
                if (active) {
                    setError(
                        cause instanceof Error
                            ? cause.message
                            : "Không thể tải dữ liệu đặt phòng.",
                    );
                }
            })
            .finally(() => {
                if (active) setLoading(false);
            });

        return () => {
            active = false;
        };
    }, [authLoading, user, router]);

    const selectedCustomer = useMemo(
        () => customers.find(
            (customer) => customer.customerId === Number(customerId),
        ),
        [customers, customerId],
    );

    async function submit(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        setError("");

        const chosenRooms = rooms.filter((room) =>
            selectedRoomIds.includes(room.roomId),
        );

        const bookingDetails = chosenRooms.map((room) => ({
            roomId: room.roomId,
            guestCount: roomGuests[room.roomId] ?? 1,
        }));

        const guestCount = bookingDetails.reduce(
            (total, detail) => total + detail.guestCount,
            0,
        );

        if (!customerId) {
            setError("Vui lòng chọn khách hàng.");
            return;
        }

        if (chosenRooms.length === 0) {
            setError("Vui lòng chọn ít nhất một phòng.");
            return;
        }

        if (new Date(checkOut) <= new Date(checkIn)) {
            setError("Ngày trả phòng phải sau ngày nhận phòng.");
            return;
        }

        const overCapacity = chosenRooms.find(
            (room) => (roomGuests[room.roomId] ?? 1) > room.capacity,
        );

        if (overCapacity) {
            setError(
                `Phòng ${overCapacity.roomName || `#${overCapacity.roomId}`} có sức chứa tối đa ${overCapacity.capacity} khách.`,
            );
            return;
        }

        setSubmitting(true);

        try {
            const booking = await createBooking({
                customerId: Number(customerId),
                expectedCheckIn: new Date(checkIn).toISOString(),
                expectedCheckOut: new Date(checkOut).toISOString(),
                guestCount,
                roomCount: bookingDetails.length,
                promotionId: null,
                servicePackageId: servicePackageId
                    ? Number(servicePackageId)
                    : null,
                note: note.trim() || null,
                bookingDetails,
            });

            setCreated(booking);
        } catch (cause) {
            setError(
                cause instanceof Error
                    ? cause.message
                    : "Không thể tạo đặt phòng.",
            );
        } finally {
            setSubmitting(false);
        }
    }

    if (
        authLoading ||
        !user ||
        (user.roleName !== "Staff" && user.roleName !== "Admin")
    ) {
        return (
            <div className="mx-auto flex min-h-96 items-center justify-center gap-3 text-muted-foreground">
                <LoaderCircle className="size-5 animate-spin" />
                Đang kiểm tra phiên đăng nhập…
            </div>
        );
    }

    if (created) {
        return (
            <section className="mx-auto max-w-3xl px-4 py-12">
                <div className="rounded-2xl border bg-card p-8 text-center sm:p-12">
                    <CheckCircle2 className="mx-auto size-14 text-emerald-600" />

                    <p className="mt-4 text-sm font-semibold uppercase tracking-widest text-primary">
                        Đặt phòng tại quầy
                    </p>

                    <h1 className="mt-2 text-3xl font-bold">
                        Đã tạo đặt phòng #{created.bookingId}
                    </h1>

                    <p className="mt-3 text-muted-foreground">
                        Khách hàng: {created.fullName || selectedCustomer?.fullName || "—"}
                    </p>

                    <div className="mt-6 rounded-xl bg-muted/50 p-5 text-left text-sm">
                        <p>
                            Nhận phòng:{" "}
                            <strong>
                                {new Date(created.expectedCheckIn).toLocaleString("vi-VN")}
                            </strong>
                        </p>

                        <p className="mt-2">
                            Trả phòng:{" "}
                            <strong>
                                {new Date(created.expectedCheckOut).toLocaleString("vi-VN")}
                            </strong>
                        </p>

                        <p className="mt-2">
                            {created.guestCount} khách · {created.roomCount} phòng
                        </p>

                        <p className="mt-2">
                            Tổng tiền dự kiến:{" "}
                            <strong>{money(created.totalAmount)}</strong>
                        </p>

                        <p className="mt-2">
                            Tiền đặt cọc:{" "}
                            <strong>{money(created.depositAmount)}</strong>
                        </p>
                    </div>

                    <div className="mt-7 flex flex-wrap justify-center gap-3">
                        <Link
                            href={`/staff/bookings/${created.bookingId}`}
                            className="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground"
                        >
                            Xem đặt phòng
                        </Link>

                        <Link
                            href="/staff/bookings"
                            className="rounded-lg border px-4 py-2 text-sm font-medium"
                        >
                            Danh sách đặt phòng
                        </Link>
                    </div>
                </div>
            </section>
        );
    }

    return (
        <section className="mx-auto max-w-4xl space-y-6 px-4 py-8 sm:px-6">
            <Link
                href="/staff/bookings"
                className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground"
            >
                <ArrowLeft className="size-4" />
                Quay lại danh sách đặt phòng
            </Link>

            <div>
                <p className="text-sm font-semibold uppercase tracking-widest text-primary">
                    Lễ tân
                </p>

                <h1 className="mt-2 text-3xl font-bold">
                    Đặt phòng tại quầy
                </h1>

                <p className="mt-2 text-muted-foreground">
                    Chọn hồ sơ khách hàng, thời gian lưu trú và các phòng cần đặt.
                </p>
            </div>

            {loading ? (
                <div className="flex min-h-48 items-center justify-center gap-3 text-muted-foreground">
                    <LoaderCircle className="size-5 animate-spin" />
                    Đang tải dữ liệu…
                </div>
            ) : (
                <form
                    onSubmit={submit}
                    className="space-y-6 rounded-2xl border bg-card p-5 sm:p-8"
                >
                    {error && (
                        <div
                            role="alert"
                            className="flex gap-2 rounded-xl border border-destructive/30 bg-destructive/5 p-4 text-sm text-destructive"
                        >
                            <CircleAlert className="size-5 shrink-0" />
                            {error}
                        </div>
                    )}

                    <label className="block max-w-2xl space-y-2 text-sm font-medium">
                        Khách hàng

                        <select
                            required
                            value={customerId}
                            onChange={(event) => setCustomerId(event.target.value)}
                            className="h-11 w-full rounded-md border bg-background px-3"
                        >
                            <option value="">Chọn hồ sơ khách hàng</option>

                            {customers.map((customer) => (
                                <option
                                    key={customer.customerId}
                                    value={customer.customerId}
                                >
                                    {customer.fullName || `Khách hàng #${customer.customerId}`}
                                    {" · "}
                                    {customer.phoneNumber || customer.email || `Mã #${customer.customerId}`}
                                </option>
                            ))}
                        </select>

                        {customers.length === 0 && (
                            <span className="block text-xs text-muted-foreground">
                                Chưa có hồ sơ khách hàng. Hãy tạo hồ sơ trước khi đặt phòng.
                            </span>
                        )}
                    </label>

                    <div className="grid gap-5 sm:grid-cols-2">
                        <label className="space-y-2 text-sm font-medium">
                            Ngày giờ nhận phòng

                            <Input
                                type="datetime-local"
                                required
                                min={todayInput()}
                                value={checkIn}
                                onChange={(event) => setCheckIn(event.target.value)}
                            />
                        </label>

                        <label className="space-y-2 text-sm font-medium">
                            Ngày giờ trả phòng

                            <Input
                                type="datetime-local"
                                required
                                min={checkIn || todayInput()}
                                value={checkOut}
                                onChange={(event) => setCheckOut(event.target.value)}
                            />
                        </label>
                    </div>

                    <fieldset className="space-y-3">
                        <legend className="text-sm font-medium">
                            Phòng và số khách theo phòng
                        </legend>

                        <p className="text-xs text-muted-foreground">
                            Tình trạng trống được kiểm tra theo khoảng ngày khi tạo đặt phòng.
                        </p>

                        <p className="text-xs text-muted-foreground">
                            Đã chọn {selectedRoomIds.length} phòng · tổng cộng{" "}
                            {selectedRoomIds.reduce(
                                (total, id) => total + (roomGuests[id] ?? 1),
                                0,
                            )}{" "}
                            khách
                        </p>

                        {rooms.length === 0 ? (
                            <p className="rounded-lg border border-dashed p-4 text-sm text-muted-foreground">
                                Hiện không có phòng đang hoạt động.
                            </p>
                        ) : (
                            <div className="grid gap-2 sm:grid-cols-2">
                                {rooms.map((room) => {
                                    const selected = selectedRoomIds.includes(room.roomId);

                                    return (
                                        <div
                                            key={room.roomId}
                                            className="rounded-lg border p-3"
                                        >
                                            <label className="flex items-start gap-2 text-sm">
                                                <input
                                                    type="checkbox"
                                                    checked={selected}
                                                    onChange={(event) => {
                                                        if (event.target.checked) {
                                                            setSelectedRoomIds((current) => [
                                                                ...current,
                                                                room.roomId,
                                                            ]);

                                                            setRoomGuests((current) => ({
                                                                ...current,
                                                                [room.roomId]:
                                                                    current[room.roomId] ?? 1,
                                                            }));
                                                        } else {
                                                            setSelectedRoomIds((current) =>
                                                                current.filter(
                                                                    (id) => id !== room.roomId,
                                                                ),
                                                            );
                                                        }
                                                    }}
                                                />

                                                <span>
                                                    <strong>
                                                        {room.roomName || `Phòng ${room.roomId}`}
                                                    </strong>

                                                    <span className="block text-xs text-muted-foreground">
                                                        {room.roomTypeName || ""} · tối đa{" "}
                                                        {room.capacity} khách ·{" "}
                                                        {money(room.pricePerNight)}/đêm
                                                    </span>
                                                </span>
                                            </label>

                                            {selected && (
                                                <label className="mt-3 flex items-center gap-2 text-xs text-muted-foreground">
                                                    Số khách

                                                    <Input
                                                        className="w-24"
                                                        type="number"
                                                        min={1}
                                                        max={room.capacity}
                                                        required
                                                        value={roomGuests[room.roomId] ?? 1}
                                                        onChange={(event) =>
                                                            setRoomGuests((current) => ({
                                                                ...current,
                                                                [room.roomId]: Number(
                                                                    event.target.value,
                                                                ),
                                                            }))
                                                        }
                                                    />
                                                </label>
                                            )}
                                        </div>
                                    );
                                })}
                            </div>
                        )}
                    </fieldset>

                    <label className="block max-w-lg space-y-2 text-sm font-medium">
                        Gói dịch vụ (không bắt buộc)

                        <select
                            value={servicePackageId}
                            onChange={(event) => setServicePackageId(event.target.value)}
                            className="h-11 w-full rounded-md border bg-background px-3"
                        >
                            <option value="">Không chọn gói dịch vụ</option>

                            {packages.map((item) => (
                                <option
                                    key={item.servicePackageId}
                                    value={item.servicePackageId}
                                >
                                    {item.packageName} · {money(item.packagePrice)}
                                </option>
                            ))}
                        </select>

                        {packages.length === 0 && !packageError && (
                            <span className="block text-xs text-muted-foreground">
                                Hiện không có gói dịch vụ.
                            </span>
                        )}

                        {packageError && (
                            <span
                                role="status"
                                className="block text-xs text-amber-700"
                            >
                                Không thể tải gói dịch vụ ({packageError}); có thể tiếp tục mà không chọn gói.
                            </span>
                        )}
                    </label>

                    <label className="block space-y-2 text-sm font-medium">
                        Ghi chú (không bắt buộc)

                        <textarea
                            maxLength={500}
                            value={note}
                            onChange={(event) => setNote(event.target.value)}
                            className="min-h-24 w-full rounded-md border bg-background px-3 py-2 text-sm"
                        />
                    </label>

                    <Button
                        type="submit"
                        disabled={
                            submitting ||
                            loading ||
                            customers.length === 0 ||
                            rooms.length === 0 ||
                            selectedRoomIds.length === 0
                        }
                        className="w-full sm:w-auto"
                    >
                        {submitting ? "Đang tạo đặt phòng…" : "Tạo đặt phòng"}
                    </Button>
                </form>
            )}
        </section>
    );
}