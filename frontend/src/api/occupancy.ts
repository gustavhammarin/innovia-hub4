import { api } from "./client";
import type { CurrentOccupancy, RangeOccupancy } from "./types";

export const occupancyApi = {
  getCurrent: () => api.get<CurrentOccupancy>("/occupancy/current"),
  getByRange: (from: string, to: string) =>
    api.get<RangeOccupancy>(`/occupancy/range?from=${from}&to=${to}`),
};
