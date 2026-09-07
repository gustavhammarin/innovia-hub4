import { api } from "./client";
import type { Me } from "./types";

export const authApi = {
  login: (email: string, password: string) =>
    api.post<void>("/auth/login", { email, password }),
  register: (email: string, password: string) =>
    api.post<void>("/auth/register", { email, password }),
  logout: () => api.post<void>("/auth/logout"),
  me: () => api.get<Me>("/auth/me"),
};
