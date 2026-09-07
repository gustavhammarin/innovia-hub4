import { useState } from "react";
import { Modal } from "../../components/Modal";
import { ApiError } from "../../api/client";
import type { Resource } from "../../api/types";
import { useResourceAvailability } from "../../hooks/useBookings";
import { useCreateBooking } from "../../hooks/useBookingMutations";
import { useResourceTypes } from "../../hooks/useResources";
import { useWeekRange } from "../../hooks/useWeekRange";
import { useSlotSelection } from "../../hooks/useSlotSelection";
import { groupSlotsByDate } from "../../lib/availability";
import { formatDate, formatTime } from "../../lib/date";

export function BookingModal({ resource, onClose }: { resource: Resource; onClose: () => void }) {
  const [error, setError] = useState<string | null>(null);
  const { fromDate, toDate, nextWeek, previousWeek } = useWeekRange();

  const { data, isLoading } = useResourceAvailability(resource.id, fromDate, toDate);
  const { data: resourceTypes } = useResourceTypes();
  const maxDurationMinutes = resourceTypes?.find((t) => t.id === resource.resourceTypeId)
    ?.maxDurationMinutes;

  const slotsByDate = groupSlotsByDate(data?.slots ?? []);
  const { toggleSlot, clear, isSelected, rangeStart, rangeEnd, duration, exceedsMax } =
    useSlotSelection(slotsByDate, maxDurationMinutes);

  const bookMutation = useCreateBooking();

  function handleBook() {
    if (!rangeStart || !rangeEnd) return;
    setError(null);
    bookMutation.mutate(
      { resourceId: resource.id, startsAt: rangeStart.startUtc, endsAt: rangeEnd.endUtc },
      {
        onSuccess: onClose,
        onError: (err) => setError(err instanceof ApiError ? err.message : "Kunde inte boka"),
      }
    );
  }

  return (
    <Modal title={`Boka: ${resource.name}`} onClose={onClose}>
      <p className="text-xs text-gray-500 dark:text-gray-400 mb-3">
        Klicka en starttid, klicka sedan en sluttid för att boka flera timmar i följd.
      </p>

      <div className="flex items-center justify-between mb-4">
        <button
          onClick={previousWeek}
          className="text-sm text-gray-500 hover:text-gray-900 dark:hover:text-gray-100"
        >
          ← Föregående vecka
        </button>
        <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
          {formatDate(`${fromDate}T00:00:00`)} – {formatDate(`${toDate}T00:00:00`)}
        </span>
        <button
          onClick={nextWeek}
          className="text-sm text-gray-500 hover:text-gray-900 dark:hover:text-gray-100"
        >
          Nästa vecka →
        </button>
      </div>

      {isLoading && <p className="text-sm text-gray-500">Laddar lediga tider...</p>}
      {error && <p className="text-sm text-red-600 mb-3">{error}</p>}

      {!isLoading && slotsByDate.size === 0 && (
        <p className="text-sm text-gray-500">Inga lediga tider denna vecka.</p>
      )}

      <div className="space-y-4 max-h-80 overflow-y-auto">
        {Array.from(slotsByDate.entries()).map(([dateKey, slots]) => (
          <div key={dateKey}>
            <h3 className="text-xs font-semibold uppercase text-gray-400 mb-2">
              {formatDate(`${dateKey}T00:00:00`)}
            </h3>
            <div className="grid grid-cols-4 gap-2">
              {slots.map((slot, index) => {
                const selected = isSelected(dateKey, index);
                return (
                  <button
                    key={slot.startUtc}
                    disabled={!slot.isAvailable || bookMutation.isPending}
                    onClick={() => {
                      setError(null);
                      toggleSlot(dateKey, index);
                    }}
                    className={`rounded-md px-2 py-1.5 text-sm font-medium border transition-colors ${
                      selected
                        ? "border-indigo-600 bg-indigo-600 text-white"
                        : slot.isAvailable
                          ? "border-indigo-200 text-indigo-700 hover:bg-indigo-50 dark:border-indigo-800 dark:text-indigo-300 dark:hover:bg-indigo-900/30"
                          : "border-gray-100 text-gray-300 line-through dark:border-gray-800 dark:text-gray-600 cursor-not-allowed"
                    }`}
                  >
                    {formatTime(slot.startUtc)}
                  </button>
                );
              })}
            </div>
          </div>
        ))}
      </div>

      {rangeStart && rangeEnd && (
        <div className="mt-4 flex items-center justify-between rounded-lg border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50 px-4 py-3">
          <div>
            <p className="text-sm font-medium text-gray-900 dark:text-gray-100">
              {formatDate(rangeStart.startUtc)} · {formatTime(rangeStart.startUtc)}–
              {formatTime(rangeEnd.endUtc)}
            </p>
            <p className="text-xs text-gray-500 dark:text-gray-400">
              {duration} minuter
              {maxDurationMinutes != null && ` (max ${maxDurationMinutes} min)`}
            </p>
            {exceedsMax && (
              <p className="text-xs text-red-600">Överskrider max bokningslängd för denna resurstyp.</p>
            )}
          </div>
          <div className="flex gap-2">
            <button
              onClick={clear}
              className="text-sm font-medium text-gray-500 hover:text-gray-900 dark:hover:text-gray-100"
            >
              Rensa
            </button>
            <button
              onClick={handleBook}
              disabled={bookMutation.isPending || exceedsMax}
              className="rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50"
            >
              Boka
            </button>
          </div>
        </div>
      )}
    </Modal>
  );
}
