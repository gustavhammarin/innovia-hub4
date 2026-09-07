import { useQuery } from "@tanstack/react-query";
import { resourcesApi } from "../api/resources";
import { resourceTypesApi } from "../api/resourceTypes";

export function useResources() {
  return useQuery({ queryKey: ["resources"], queryFn: resourcesApi.list });
}

export function useResourcesAdmin() {
  return useQuery({ queryKey: ["resourcesAdmin"], queryFn: resourcesApi.listAdmin });
}

export function useResourceTypes() {
  return useQuery({ queryKey: ["resourceTypes"], queryFn: resourceTypesApi.list });
}
