import { useQuery } from "@tanstack/react-query";
import { occupancyApi } from "../api/occupancy";

export function useCurrentOccupancy() {
  return useQuery({
    queryKey: ["occupancy", "current"],
    queryFn: occupancyApi.getCurrent,
  });
}

export function useRangeOccupancy(from: string, to: string) {
  return useQuery({
    queryKey: ["occupancy", "range", from, to],
    queryFn: () => occupancyApi.getByRange(from, to),
    enabled: Boolean(from && to),
  });
}
