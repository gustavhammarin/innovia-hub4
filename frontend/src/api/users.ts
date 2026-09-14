import { api } from "./client";
import type { AppUser, UserFormData } from "./types";

export const usersApi = {
  list: (search?: string) => {
    const qs = search ? `?search=${encodeURIComponent(search)}` : "";
    return api.get<AppUser[]>(`/admin/users${qs}`);
  },
  create: (data: UserFormData) => api.post<AppUser>("/admin/users", data),
  update: (id: string, data: UserFormData) => api.put(`/admin/users/${id}`, data),
  remove: (id: string) => api.delete<{ cancelledBookingCount: number; preservedBookingCount: number }>(`/admin/users/${id}`),
};
