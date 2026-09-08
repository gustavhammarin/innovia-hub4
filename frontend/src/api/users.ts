import { api } from "./client";
import type { AppUser } from "./types";

export const usersApi = {
  list: (search?: string) => {
    const qs = search ? `?search=${encodeURIComponent(search)}` : "";
    return api.get<AppUser[]>(`/users${qs}`);
  },
};
