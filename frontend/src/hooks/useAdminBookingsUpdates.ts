import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import {
  ensureStarted,
  getBookingHubConnection,
  joinAdminBookingsGroup,
  leaveAdminBookingsGroup,
} from "../lib/bookingHubConnection";

const BookingEvents = {
  BookingCreated: "BookingCreated",
  BookingCancelled: "BookingCancelled",
  BookingUpdated: "BookingUpdated",
} as const;

export function useAdminBookingUpdates() {
  const queryClient = useQueryClient();

  useEffect(() => {
    const conn = getBookingHubConnection();

    function invalidate() {
      queryClient.invalidateQueries({ queryKey: ["allBookings"] });
    }

    conn.on(BookingEvents.BookingCreated, invalidate);
    conn.on(BookingEvents.BookingCancelled, invalidate);
    conn.on(BookingEvents.BookingUpdated, invalidate);

    (async () => {
      await ensureStarted(conn);
      await joinAdminBookingsGroup(conn);
    })();
    return () => {
      conn.off(BookingEvents.BookingCreated, invalidate);
      conn.off(BookingEvents.BookingCancelled, invalidate);
      conn.off(BookingEvents.BookingUpdated, invalidate);
      leaveAdminBookingsGroup(conn);
    };
  }, [queryClient]);
}
