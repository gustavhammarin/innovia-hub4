import { useState, type ReactNode } from "react";
import { ApiError } from "../../api/client";
import type { Booking } from "../../api/types";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { useMyBookings } from "../../hooks/useBookings";
import { useCancelBooking } from "../../hooks/useBookingMutations";
import { splitBookings } from "../../lib/bookingGrouping";
import { formatDateTime } from "../../lib/date";

export function MyBookingsPage() {
  const [error, setError] = useState<string | null>(null);
  const [cancelTarget, setCancelTarget] = useState<Booking | null>(null);
  const [showCancelled, setShowCancelled] = useState(false);
  const bookingsQuery = useMyBookings();
  const cancelMutation = useCancelBooking();

  function confirmCancel() {
    if (!cancelTarget) return;
    setError(null);
    cancelMutation.mutate(cancelTarget.id, {
      onSuccess: () => setCancelTarget(null),
      onError: (err) => setError(err instanceof ApiError ? err.message : "Kunde inte avboka"),
    });
  }

  const { upcoming, past, cancelled } = splitBookings(bookingsQuery.data ?? []);

  return (
    <div>
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100 mb-6">Mina bokningar</h1>
      {bookingsQuery.isLoading && <p className="text-gray-500">Laddar...</p>}

      <Section title="Kommande" empty="Inga kommande bokningar.">
        {upcoming.map((b) => (
          <BookingRow
            key={b.id}
            resourceName={b.resource.name}
            when={`${formatDateTime(b.startsAt)} – ${formatDateTime(b.endsAt)}`}
            onCancel={() => setCancelTarget(b)}
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
        <section className="mb-8">
          <button
            onClick={() => setShowCancelled((v) => !v)}
            className="text-sm font-semibold uppercase tracking-wide text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 mb-3"
          >
            {showCancelled ? "Dölj avbokade" : `Visa avbokade (${cancelled.length})`}
          </button>
          {showCancelled && (
            <div className="space-y-2">
              {cancelled.map((b) => (
                <BookingRow
                  key={b.id}
                  resourceName={b.resource.name}
                  when={`${formatDateTime(b.startsAt)} – ${formatDateTime(b.endsAt)}`}
                  cancelledLabel
                />
              ))}
            </div>
          )}
        </section>
      )}

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
            {formatDateTime(cancelTarget.startsAt)} – {formatDateTime(cancelTarget.endsAt)}
          </p>
          <p className="mt-2">Bokningen går inte att återställa efter avbokning.</p>
        </ConfirmDialog>
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
  cancelledLabel,
}: {
  resourceName: string;
  when: string;
  onCancel?: () => void;
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
          className="text-sm font-medium text-red-600 hover:text-red-700"
        >
          Avboka
        </button>
      )}
    </div>
  );
}
