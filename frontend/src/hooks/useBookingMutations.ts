import { useMutation, useQueryClient } from "@tanstack/react-query";
import { bookingsApi } from "../api/bookings";

export function useCreateBooking() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { resourceId: string; startsAt: string; endsAt: string }) =>
      bookingsApi.create(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["myBookings"] }),
  });
}

export function useCancelBooking() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => bookingsApi.cancel(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["myBookings"] });
      queryClient.invalidateQueries({ queryKey: ["allBookings"] });
    },
  });
}
