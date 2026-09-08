import { useState } from "react";
import { ApiError } from "../../api/client";
import type { Booking } from "../../api/types";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { useAllBookings } from "../../hooks/useBookings";
import { useCancelBooking } from "../../hooks/useBookingMutations";
import { sortByStartDescending } from "../../lib/bookingGrouping";
import { formatDateTime } from "../../lib/date";
import { useAdminBookingUpdates } from "../../hooks/useAdminBookingsUpdates";

export function BookingsAdminPage() {
  useAdminBookingUpdates();

  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [cancelTarget, setCancelTarget] = useState<Booking | null>(null);

  const bookingsQuery = useAllBookings({
    from: from || undefined,
    to: to || undefined,
  });
  const cancelMutation = useCancelBooking();

  function confirmCancel() {
    if (!cancelTarget) return;
    setError(null);
    cancelMutation.mutate(cancelTarget.id, {
      onSuccess: () => setCancelTarget(null),
      onError: (err) => setError(err instanceof ApiError ? err.message : "Kunde inte avboka"),
    });
  }

  const bookings = sortByStartDescending(bookingsQuery.data ?? []);

  return (
    <div>
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100 mb-6">
        Alla bokningar
      </h1>

      <div className="flex gap-4 mb-4 items-end">
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Från
          </label>
          <input
            type="date"
            value={from}
            onChange={(e) => setFrom(e.target.value)}
            className="rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-gray-100"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Till
          </label>
          <input
            type="date"
            value={to}
            onChange={(e) => setTo(e.target.value)}
            className="rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-gray-100"
          />
        </div>
        {(from || to) && (
          <button
            onClick={() => {
              setFrom("");
              setTo("");
            }}
            className="text-sm font-medium text-gray-500 hover:text-gray-900 dark:hover:text-gray-100 pb-2"
          >
            Rensa filter
          </button>
        )}
      </div>

      {bookingsQuery.isLoading && <p className="text-gray-500">Laddar...</p>}

      <div className="overflow-x-auto rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900">
        <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-800 text-sm">
          <thead>
            <tr className="text-left text-xs font-semibold uppercase text-gray-400">
              <th className="px-4 py-3">Resurs</th>
              <th className="px-4 py-3">Användare</th>
              <th className="px-4 py-3">Tid</th>
              <th className="px-4 py-3">Status</th>
              <th className="px-4 py-3 text-right">Åtgärder</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
            {bookings.map((b) => (
              <tr key={b.id}>
                <td className="px-4 py-3 font-medium text-gray-900 dark:text-gray-100">
                  {b.resource.name}
                </td>
                <td className="px-4 py-3 text-gray-600 dark:text-gray-300">
                  {b.user.email}
                </td>
                <td className="px-4 py-3 text-gray-600 dark:text-gray-300">
                  {formatDateTime(b.startsAt)} – {formatDateTime(b.endsAt)}
                </td>
                <td className="px-4 py-3">
                  {b.cancelledAt ? (
                    <span className="text-xs font-medium text-red-500">
                      Avbokad
                    </span>
                  ) : (
                    <span className="text-xs font-medium text-green-600">
                      Aktiv
                    </span>
                  )}
                </td>
                <td className="px-4 py-3 text-right">
                  {!b.cancelledAt && (
                    <button
                      onClick={() => setCancelTarget(b)}
                      className="text-red-600 hover:text-red-500 font-medium"
                    >
                      Avboka
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {bookings.length === 0 && !bookingsQuery.isLoading && (
          <p className="text-sm text-gray-500 px-4 py-6">
            Inga bokningar hittades.
          </p>
        )}
      </div>

      {cancelTarget && (
        <ConfirmDialog
          title="Avboka?"
          confirmLabel="Avboka"
          cancelLabel="Behåll bokningen"
          danger
          isPending={cancelMutation.isPending}
          error={error}
          onConfirm={confirmCancel}
          onClose={() => {
            setCancelTarget(null);
            setError(null);
          }}
        >
          <p>
            <span className="font-medium text-gray-900 dark:text-gray-100">
              {cancelTarget.resource.name}
            </span>
            <br />
            {cancelTarget.user.email}
            <br />
            {formatDateTime(cancelTarget.startsAt)} – {formatDateTime(cancelTarget.endsAt)}
          </p>
          <p className="mt-2">Bokningen går inte att återställa efter avbokning.</p>
        </ConfirmDialog>
      )}
    </div>
  );
}
