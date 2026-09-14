import { api } from "./client";
import type { ResourceType } from "./types";

export const resourceTypesApi = {
  list: () => api.get<ResourceType[]>("/resource-types"),
  create: (data: { name: string; maxDurationMinutes: number; maxAdvanceDays: number }) =>
    api.post<ResourceType>("/resource-types", data),
  update: (id: string, data: { name: string; maxDurationMinutes: number; maxAdvanceDays: number }) =>
    api.put<ResourceType>(`/resource-types/${id}`, data),
};
