import { useMutation, useQueryClient } from "@tanstack/react-query";
import { resourcesApi } from "../api/resources";
import { resourceTypesApi } from "../api/resourceTypes";

export type ResourceStatusAction = "online" | "offline" | "maintenance" | "archive" | "unarchive";

function runResourceStatusAction(id: string, action: ResourceStatusAction) {
  switch (action) {
    case "online":
      return resourcesApi.setOnline(id);
    case "offline":
      return resourcesApi.setOffline(id);
    case "maintenance":
      return resourcesApi.setMaintenance(id);
    case "archive":
      return resourcesApi.remove(id);
    case "unarchive":
      return resourcesApi.unarchive(id);
  }
}

function useInvalidateResources() {
  const queryClient = useQueryClient();
  return () => {
    queryClient.invalidateQueries({ queryKey: ["resourcesAdmin"] });
    queryClient.invalidateQueries({ queryKey: ["resources"] });
  };
}

export function useResourceStatusMutation() {
  const invalidate = useInvalidateResources();
  return useMutation({
    mutationFn: (vars: { id: string; action: ResourceStatusAction }) =>
      runResourceStatusAction(vars.id, vars.action),
    onSuccess: invalidate,
  });
}

export function useSaveResourceMutation(resourceId: string | null) {
  const invalidate = useInvalidateResources();
  return useMutation({
    mutationFn: (data: { name: string; description: string; resourceTypeId: string }) =>
      resourceId ? resourcesApi.update(resourceId, data) : resourcesApi.create(data),
    onSuccess: invalidate,
  });
}

export function useSaveResourceTypeMutation(resourceTypeId: string | null) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { name: string; maxDurationMinutes: number; maxAdvanceDays: number }) =>
      resourceTypeId
        ? resourceTypesApi.update(resourceTypeId, data)
        : resourceTypesApi.create(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["resourceTypes"] }),
  });
}
