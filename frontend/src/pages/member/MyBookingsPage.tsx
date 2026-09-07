import { useState, type ReactNode } from "react";
import { ApiError } from "../../api/client";
import { useMyBookings } from "../../hooks/useBookings";
import { useCancelBooking } from "../../hooks/useBookingMutations";
import { splitBookings } from "../../lib/bookingGrouping";
import { formatDateTime } from "../../lib/date";

export function MyBookingsPage() {
  const [error, setError] = useState<string | null>(null);
  const bookingsQuery = useMyBookings();
  const cancelMutation = useCancelBooking();

  function cancel(id: string) {
    setError(null);
    cancelMutation.mutate(id, {
      onError: (err) => setError(err instanceof ApiError ? err.message : "Kunde inte avboka"),
    });
  }

  const { upcoming, past, cancelled } = splitBookings(bookingsQuery.data ?? []);

  return (
    <div>
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100 mb-6">Mina bokningar</h1>
      {error && <p className="text-sm text-red-600 mb-4">{error}</p>}
      {bookingsQuery.isLoading && <p className="text-gray-500">Laddar...</p>}

      <Section title="Kommande" empty="Inga kommande bokningar.">
        {upcoming.map((b) => (
          <BookingRow
            key={b.id}
            resourceName={b.resource.name}
            when={`${formatDateTime(b.startsAt)} – ${formatDateTime(b.endsAt)}`}
            onCancel={() => cancel(b.id)}
            cancelling={cancelMutation.isPending}
          />
        ))}
      </Section>

      <Section title="Tidigare" empty="Inga tidigare bokningar.">
        {past.map((b) => (
          <BookingRow
            key={b.id}
            resourceName={b.resource.name}
            when={`${formatDateTime(b.startsAt)} – ${formatDateTime(b.endsAt)}`}
          />
        ))}
      </Section>

      {cancelled.length > 0 && (
        <Section title="Avbokade" empty="">
          {cancelled.map((b) => (
            <BookingRow
              key={b.id}
              resourceName={b.resource.name}
              when={`${formatDateTime(b.startsAt)} – ${formatDateTime(b.endsAt)}`}
              cancelledLabel
            />
          ))}
        </Section>
      )}
    </div>
  );
}

function Section({
  title,
  empty,
  children,
}: {
  title: string;
  empty: string;
  children: ReactNode;
}) {
  const hasChildren = Array.isArray(children) ? children.length > 0 : !!children;
  return (
    <section className="mb-8">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-gray-400 mb-3">{title}</h2>
      {hasChildren ? (
        <div className="space-y-2">{children}</div>
      ) : (
        <p className="text-sm text-gray-500">{empty}</p>
      )}
    </section>
  );
}

function BookingRow({
  resourceName,
  when,
  onCancel,
  cancelling,
  cancelledLabel,
}: {
  resourceName: string;
  when: string;
  onCancel?: () => void;
  cancelling?: boolean;
  cancelledLabel?: boolean;
}) {
  return (
    <div className="flex items-center justify-between rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 px-4 py-3">
      <div>
        <p className="font-medium text-gray-900 dark:text-gray-100">{resourceName}</p>
        <p className="text-sm text-gray-500 dark:text-gray-400">{when}</p>
      </div>
      {cancelledLabel && (
        <span className="text-xs font-medium text-red-500">Avbokad</span>
      )}
      {onCancel && (
        <button
          onClick={onCancel}
          disabled={cancelling}
          className="text-sm font-medium text-red-600 hover:text-red-700 disabled:opacity-50"
        >
          Avboka
        </button>
      )}
    </div>
  );
}
