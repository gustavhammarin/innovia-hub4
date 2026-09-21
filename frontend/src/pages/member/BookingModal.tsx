import { useState } from "react";
import { Modal } from "../../components/Modal";
import { DateField } from "../../components/DateField";
import { DayNav } from "../../components/booking/DayNav";
import { SlotGrid } from "../../components/booking/SlotGrid";
import { UserTargetPicker } from "../../components/booking/UserTargetPicker";
import { BookingPreviewStep } from "../../components/booking/BookingPreviewStep";
import { BookingConfirmedStep } from "../../components/booking/BookingConfirmedStep";
import { ApiError } from "../../api/client";
import type { AppUser, Resource } from "../../api/types";
import { useResourceAvailability } from "../../hooks/useBookings";
import { useCreateBooking } from "../../hooks/useBookingMutations";
import { useResourceTypes } from "../../hooks/useResources";
import { useDayRange } from "../../hooks/useDayRange";
import { useSlotSelection } from "../../hooks/useSlotSelection";
import { groupSlotsByDate } from "../../lib/availability";
import { formatDate, formatTime } from "../../lib/date";
import { useResourceBookingUpdates } from "../../hooks/useResourceBookingUpdates";
import { useAuth } from "../../auth/AuthContext";

type Step = "select" | "preview" | "confirmed";

export function BookingModal({ resource, onClose }: { resource: Resource; onClose: () => void }) {
  const { isAdmin } = useAuth();
  const [step, setStep] = useState<Step>("select");
  const [error, setError] = useState<string | null>(null);

  const { data: resourceTypes } = useResourceTypes();
  const resourceType = resourceTypes?.find((t) => t.id === resource.resourceTypeId);
  const maxDurationMinutes = resourceType?.maxDurationMinutes;

  const { date, setDate, nextDay, previousDay, isToday, isMaxDate, minDate, maxDate } =
    useDayRange(resourceType?.maxAdvanceDays);

  const [targetUser, setTargetUser] = useState<AppUser | null>(null);

  const { data, isLoading } = useResourceAvailability(resource.id, date, date);

  const slotsByDate = groupSlotsByDate(data?.slots ?? []);
  const slots = slotsByDate.get(date) ?? [];
  const { toggleSlot, clear, isSelected, rangeStart, rangeEnd, duration, exceedsMax } =
    useSlotSelection(slotsByDate, maxDurationMinutes);

  const bookMutation = useCreateBooking();
  useResourceBookingUpdates(resource.id);

  const needsTargetUser = isAdmin && !targetUser;
  const targetUserFullName = isAdmin && targetUser ? targetUser.fullName : undefined;

  function handleReview() {
    if (!rangeStart || !rangeEnd || needsTargetUser) return;
    setError(null);
    setStep("preview");
  }

  function handleConfirm() {
    if (!rangeStart || !rangeEnd) return;
    setError(null);
    bookMutation.mutate(
      {
        resourceId: resource.id,
        startsAt: rangeStart.startUtc,
        endsAt: rangeEnd.endUtc,
        ...(isAdmin && targetUser ? { userId: targetUser.id } : {}),
      },
      {
        onSuccess: () => setStep("confirmed"),
        onError: (err) => setError(err instanceof ApiError ? err.message : "Kunde inte boka"),
      }
    );
  }

  function bookAnother() {
    clear();
    setError(null);
    setStep("select");
  }

  return (
    <Modal
      title={step === "confirmed" ? "Bokning bekräftad" : `Boka: ${resource.name}`}
      onClose={onClose}
      size="lg"
    >
      {step === "select" && (
        <>
          {isAdmin && <UserTargetPicker targetUser={targetUser} onChange={setTargetUser} />}

          <p className="text-xs text-gray-500 dark:text-gray-400 mb-3">
            Klicka en starttid, klicka sedan en sluttid för att boka flera timmar i följd.
          </p>

          <div className="mb-4 space-y-3">
            <DayNav
              dateLabel={formatDate(`${date}T00:00:00`)}
              onPrev={previousDay}
              onNext={nextDay}
              prevDisabled={isToday}
              nextDisabled={isMaxDate}
            />
            <DateField
              value={date}
              minDate={minDate}
              maxDate={maxDate}
              onChange={setDate}
              resourceId={resource.id}
            />
            <div className="flex items-center justify-center gap-1.5 text-[11px] text-gray-400 dark:text-gray-500">
              <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />
              <span>Lediga tider</span>
            </div>
            {maxDate && (
              <p className="text-center text-[11px] text-gray-400 dark:text-gray-500">
                Går att boka max {resourceType?.maxAdvanceDays} dagar fram i tiden
              </p>
            )}
          </div>

          {isLoading && <p className="text-sm text-gray-500">Laddar lediga tider...</p>}
          {error && <p className="text-sm text-red-600 mb-3">{error}</p>}

          {!isLoading && slots.length === 0 && (
            <p className="text-sm text-gray-500">Inga lediga tider denna dag.</p>
          )}

          <SlotGrid
            slots={slots.map((s) => ({ startUtc: s.startUtc, endUtc: s.endUtc, available: s.isAvailable }))}
            isSelected={(index) => isSelected(date, index)}
            onToggle={(index) => {
              setError(null);
              toggleSlot(date, index);
            }}
          />

          {rangeStart && rangeEnd && (
            <div className="mt-4 flex items-center justify-between rounded-lg border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50 px-4 py-3">
              <div>
                <p className="text-sm font-medium text-gray-900 dark:text-gray-100">
                  {formatTime(rangeStart.startUtc)}–{formatTime(rangeEnd.endUtc)}
                </p>
                <p className="text-xs text-gray-500 dark:text-gray-400">
                  {duration} minuter
                  {maxDurationMinutes != null && ` (max ${maxDurationMinutes} min)`}
                </p>
                {exceedsMax && (
                  <p className="text-xs text-red-600">
                    Överskrider max bokningslängd för denna resurstyp.
                  </p>
                )}
                {needsTargetUser && (
                  <p className="text-xs text-red-600">Välj en användare att boka åt.</p>
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
                  onClick={handleReview}
                  disabled={exceedsMax || needsTargetUser}
                  className="rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50"
                >
                  Granska bokning
                </button>
              </div>
            </div>
          )}
        </>
      )}

      {step === "preview" && rangeStart && rangeEnd && (
        <BookingPreviewStep
          resourceName={resource.name}
          targetUserFullName={targetUserFullName}
          dateLabel={formatDate(`${date}T00:00:00`)}
          timeLabel={`${formatTime(rangeStart.startUtc)}–${formatTime(rangeEnd.endUtc)}`}
          durationMinutes={duration}
          error={error}
          isPending={bookMutation.isPending}
          onBack={() => setStep("select")}
          onConfirm={handleConfirm}
        />
      )}

      {step === "confirmed" && rangeStart && rangeEnd && (
        <BookingConfirmedStep
          summary={`${resource.name} · ${formatDate(`${date}T00:00:00`)} · ${formatTime(rangeStart.startUtc)}–${formatTime(rangeEnd.endUtc)}`}
          targetUserFullName={targetUserFullName}
          onBookAnother={bookAnother}
          onClose={onClose}
        />
      )}
    </Modal>
  );
}
