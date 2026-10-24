import { fetchApi } from "./client";

import type {
    RoomCreateDto,
    RoomUpdateDto,
    RoomResponseDto,
    RoomFilterDto,
} from "@/types/room";

import { getPublicRoomImages } from "@/app/api/room-image.api";

const attachRoomImages = async (
    rooms: RoomResponseDto[],
): Promise<RoomResponseDto[]> => {
    const publicImages = await getPublicRoomImages().catch(() => []);
    const safeImages = Array.isArray(publicImages) ? publicImages : [];

    return (Array.isArray(rooms) ? rooms : []).map((room) => {
        const images = safeImages.filter((image) => image.roomId === room.roomId);

        return {
            ...room,
            roomImages: images.length > 0 ? images : (room.roomImages ?? []),
        };
    });
};

export const getAll = async (): Promise<RoomResponseDto[]> => {
    const rooms = await fetchApi<RoomResponseDto[]>("/Room");

    return attachRoomImages(rooms);
};

export const getRoomById = async (roomId: number): Promise<RoomResponseDto> => {
    const room = await fetchApi<RoomResponseDto>(`/Room/${roomId}`);
    const [withImages] = await attachRoomImages([room]);

    return withImages;
};

export const getDeletedRooms = () =>
    fetchApi<RoomResponseDto[]>("/Room/deleted");

export const softDeleteRoom = (roomId: number) =>
    fetchApi<void>(`/Room/${roomId}/soft-delete`, {
        method: "PATCH",
    });

export const restoreRoom = (roomId: number) =>
    fetchApi<void>(`/Room/${roomId}/restore`, {
        method: "PATCH",
    });

export const deleteRoom = (roomId: number) =>
    fetchApi<void>(`/Room/${roomId}`, {
        method: "DELETE",
    });

export const createRoom = (data: RoomCreateDto) =>
    fetchApi<RoomResponseDto>("/Room", {
        method: "POST",
        body: JSON.stringify(data),
    });

export const updateRoom = (roomId: number, data: RoomUpdateDto) =>
    fetchApi<void>(`/Room/${roomId}`, {
        method: "PUT",
        body: JSON.stringify(data),
    });

export const filterRooms = async (
    filters: RoomFilterDto,
): Promise<RoomResponseDto[]> => {
    const params = new URLSearchParams();

    if (filters.roomTypeId != null) {
        params.set("RoomTypeId", String(filters.roomTypeId));
    }

    if (filters.minPrice != null) {
        params.set("MinPrice", String(filters.minPrice));
    }

    if (filters.maxPrice != null) {
        params.set("MaxPrice", String(filters.maxPrice));
    }

    if (filters.minCapacity != null) {
        params.set("MinCapacity", String(filters.minCapacity));
    }

    if (filters.maxCapacity != null) {
        params.set("MaxCapacity", String(filters.maxCapacity));
    }

    const query = params.toString();

    const rooms = await fetchApi<RoomResponseDto[]>(
        `/Room/filter${query ? `?${query}` : ""}`,
    );

    return attachRoomImages(rooms);
};