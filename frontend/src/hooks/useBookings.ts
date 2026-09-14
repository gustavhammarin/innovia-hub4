import { useQuery } from "@tanstack/react-query";
import { bookingsApi } from "../api/bookings";
import { availabilityApi } from "../api/availability";

export function useMyBookings() {
  return useQuery({ queryKey: ["myBookings"], queryFn: bookingsApi.myBookings });
}

export function useAllBookings(filters: { from?: string; to?: string }) {
  return useQuery({
    queryKey: ["allBookings", filters.from, filters.to],
    queryFn: () => bookingsApi.listAll(filters),
  });
}

export function useResourceAvailability(resourceId: string, from: string, to: string) {
  return useQuery({
    queryKey: ["availability", resourceId, from, to],
    queryFn: () => availabilityApi.getForResource(resourceId, from, to),
  });
}
