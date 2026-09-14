import { useQueries } from "@tanstack/react-query";
import { availabilityApi } from "../api/availability";
import type { AvailabilitySlot } from "../api/types";

export function useResourcesAvailability(resourceIds: string[], from: string, to: string) {
  const results = useQueries({
    queries: resourceIds.map((id) => ({
      queryKey: ["availability", id, from, to],
      queryFn: () => availabilityApi.getForResource(id, from, to),
    })),
  });

  const isLoading = results.some((r) => r.isLoading);
  const byResourceId = new Map<string, AvailabilitySlot[]>(
    resourceIds.map((id, i) => [id, results[i].data?.slots ?? []])
  );

  return { isLoading, byResourceId };
}
