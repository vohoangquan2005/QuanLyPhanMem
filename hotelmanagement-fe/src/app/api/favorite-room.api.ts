import { fetchApi } from "./client";
import type {
    FavoriteRoomCreateDto,
    FavoriteRoomResponseDto,
} from "@/types/favorite-room";

export const getMyFavoriteRooms = () =>
    fetchApi<FavoriteRoomResponseDto[]>("/FavoriteRoom");

export const addFavoriteRoom = (data: FavoriteRoomCreateDto) =>
    fetchApi<FavoriteRoomResponseDto>("/FavoriteRoom", {
        method: "POST",
        body: JSON.stringify(data),
    });

export const removeFavoriteRoom = (favoriteRoomId: number) =>
    fetchApi<void>(`/FavoriteRoom/${favoriteRoomId}`, {
        method: "DELETE",
    });