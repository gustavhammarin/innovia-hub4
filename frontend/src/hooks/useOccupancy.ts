import { useState } from "react";
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

export function useOccupancyView(from: string, to: string) {
  const [view, setView] = useState<"current" | "range">("current");
  const currentQuery = useCurrentOccupancy();
  const rangeQuery = useRangeOccupancy(from, to);

  const data = view === "current" ? currentQuery.data : rangeQuery.data;
  const isLoading = view === "current" ? currentQuery.isLoading : rangeQuery.isLoading;

  return { view, setView, data, isLoading };
}
