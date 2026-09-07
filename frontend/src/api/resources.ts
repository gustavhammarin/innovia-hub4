import { api } from "./client";
import type { Resource } from "./types";

export const resourcesApi = {
  list: () => api.get<Resource[]>("/resources"),
  listAdmin: () => api.get<Resource[]>("/resources/admin"),
  getById: (id: string) => api.get<Resource>(`/resources/${id}`),
  create: (data: { name: string; description: string; resourceTypeId: string }) =>
    api.post<Resource>("/resources", data),
  update: (id: string, data: { name: string; description: string; resourceTypeId: string }) =>
    api.put<Resource>(`/resources/${id}`, data),
  remove: (id: string) => api.delete<Resource>(`/resources/${id}`),
  unarchive: (id: string) => api.post<Resource>(`/resources/${id}/unarchive`),
  setOnline: (id: string) => api.post<Resource>(`/resources/${id}/online`),
  setOffline: (id: string) => api.post<Resource>(`/resources/${id}/offline`),
  setMaintenance: (id: string) => api.post<Resource>(`/resources/${id}/maintenance`),
};
