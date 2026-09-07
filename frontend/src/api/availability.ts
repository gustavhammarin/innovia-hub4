import { api } from "./client";
import type { AvailabilityResponse } from "./types";

export const availabilityApi = {
  getForResource: (resourceId: string, from: string, to: string) =>
    api.get<AvailabilityResponse>(
      `/availability/resources/${resourceId}?from=${from}&to=${to}`
    ),
};
