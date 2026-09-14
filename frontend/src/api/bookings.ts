import { api } from "./client";
import type { Booking } from "./types";

export const bookingsApi = {
  myBookings: () => api.get<Booking[]>("/bookings/me"),
  listAll: (filters?: { userId?: string; resourceId?: string; from?: string; to?: string }) => {
    const params = new URLSearchParams();
    if (filters?.userId) params.set("userId", filters.userId);
    if (filters?.resourceId) params.set("resourceId", filters.resourceId);
    if (filters?.from) params.set("from", filters.from);
    if (filters?.to) params.set("to", filters.to);
    const qs = params.toString();
    return api.get<Booking[]>(`/bookings${qs ? `?${qs}` : ""}`);
  },
  create: (data: { resourceId: string; startsAt: string; endsAt: string; userId?: string }) =>
    api.post<Booking>("/bookings", data),
  update: (id: string, data: { resourceId: string; startsAt: string; endsAt: string }) =>
    api.put<Booking>(`/bookings/${id}`, data),
  cancel: (id: string) => api.delete<{ id: string; cancelledAt: string }>(`/bookings/${id}`),
};
