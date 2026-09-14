import type { Booking } from "../api/types";

export interface SplitBookings {
  upcoming: Booking[];
  past: Booking[];
  cancelled: Booking[];
}

export function splitBookings(bookings: Booking[], now = new Date()): SplitBookings {
  const active = bookings.filter((b) => !b.cancelledAt);

  const upcoming = active
    .filter((b) => new Date(b.startsAt) >= now)
    .sort((a, b) => new Date(a.startsAt).getTime() - new Date(b.startsAt).getTime());

  const past = active
    .filter((b) => new Date(b.startsAt) < now)
    .sort((a, b) => new Date(b.startsAt).getTime() - new Date(a.startsAt).getTime());

  const cancelled = bookings.filter((b) => b.cancelledAt);

  return { upcoming, past, cancelled };
}

export function sortByStartDescending(bookings: Booking[]): Booking[] {
  return bookings
    .slice()
    .sort((a, b) => new Date(b.startsAt).getTime() - new Date(a.startsAt).getTime());
}
