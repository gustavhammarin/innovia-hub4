import { useEffect, useState } from "react";
import { Modal } from "../../components/Modal";
import { MonthGrid } from "../../components/MonthGrid";
import { DayNav } from "../../components/booking/DayNav";
import { SlotGrid } from "../../components/booking/SlotGrid";
import { UserTargetPicker } from "../../components/booking/UserTargetPicker";
import { BookingPreviewStep } from "../../components/booking/BookingPreviewStep";
import { BookingConfirmedStep } from "../../components/booking/BookingConfirmedStep";
import { ApiError } from "../../api/client";
import type { AppUser, Resource, ResourceType } from "../../api/types";
import { useResourcesAvailability } from "../../hooks/useResourcesAvailability";
import { useCreateBooking } from "../../hooks/useBookingMutations";
import { useDayRange } from "../../hooks/useDayRange";
import { useAggregatedSlotSelection } from "../../hooks/useAggregatedSlotSelection";
import { aggregateSlotsAcrossResources, datesWithAvailability } from "../../lib/availability";
import { formatDate, formatTime } from "../../lib/date";
import { monthAvailabilityRange, shiftMonth } from "../../lib/calendarGrid";
import { useResourcesBookingUpdates } from "../../hooks/useResourceBookingUpdates";
import { useAuth } from "../../auth/AuthContext";

type Step = "time" | "resource" | "preview" | "confirmed";

export function BookingFlowModal({
  resourceType,
  resources,
  initialDate,
  onClose,
}: {
  resourceType: ResourceType;
  resources: Resource[];
  initialDate: string;
  onClose: () => void;
}) {
  const { isAdmin } = useAuth();
  const [step, setStep] = useState<Step>("time");
  const [error, setError] = useState<string | null>(null);
  const [chosenResource, setChosenResource] = useState<Resource | null>(null);

  const { date, setDate, nextDay, previousDay, isToday, isMaxDate, minDate, maxDate } = useDayRange(
    resourceType.maxAdvanceDays,
    initialDate
  );

  const [viewMonth, setViewMonth] = useState(initialDate.slice(0, 7));
  // Keep the visible month in sync when the day-arrow shortcuts cross a month boundary.
  useEffect(() => {
    setViewMonth(date.slice(0, 7));
  }, [date]);
  const canGoPrevMonth = viewMonth > minDate.slice(0, 7);
  const canGoNextMonth = !maxDate || viewMonth < maxDate.slice(0, 7);
  const monthRange = monthAvailabilityRange(viewMonth, minDate, maxDate);
  const { byResourceId: monthByResourceId, isLoading: monthLoading } = useResourcesAvailability(
    resources.map((r) => r.id),
    monthRange.from,
    monthRange.to
  );
  const availableDates = datesWithAvailability(
    resources.map((r) => r.id),
    monthByResourceId
  );
  const monthLabel = new Date(`${viewMonth}-01T00:00:00`).toLocaleDateString("sv-SE", {
    month: "long",
    year: "numeric",
  });

  const [targetUser, setTargetUser] = useState<AppUser | null>(null);

  const resourceIds = resources.map((r) => r.id);
  const { byResourceId, closedForRestOfTodayByResourceId, isLoading } = useResourcesAvailability(
    resourceIds,
    date,
    date
  );
  const slots = aggregateSlotsAcrossResources(resourceIds, byResourceId);
  const closedForRestOfToday =
    isToday &&
    resourceIds.length > 0 &&
    resourceIds.every((id) => closedForRestOfTodayByResourceId.get(id) ?? false);

  const {
    toggleSlot,
    clear,
    isSelected,
    rangeStart,
    rangeEnd,
    duration,
    exceedsMax,
    availableResourceIds,
    selectFullDay,
    isFullDaySelected,
    hasFullDayOption,
  } = useAggregatedSlotSelection(slots, resourceType.maxDurationMinutes);

  const bookMutation = useCreateBooking();
  useResourcesBookingUpdates(resourceIds);

  const candidateResources = resources.filter((r) => availableResourceIds.includes(r.id));
  const needsTargetUser = isAdmin && !targetUser;
  const targetUserFullName = isAdmin && targetUser ? targetUser.fullName : undefined;

  function goToResourceStep() {
    if (!rangeStart || !rangeEnd || needsTargetUser || exceedsMax) return;
    setError(null);
    setChosenResource(candidateResources.length === 1 ? candidateResources[0] : null);
    setStep("resource");
  }

  function goToPreview() {
    if (!chosenResource) return;
    setError(null);
    setStep("preview");
  }

  function handleConfirm() {
    if (!rangeStart || !rangeEnd || !chosenResource) return;
    setError(null);
    bookMutation.mutate(
      {
        resourceId: chosenResource.id,
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
    setChosenResource(null);
    setError(null);
    setStep("time");
  }

  const titles: Record<Step, string> = {
    time: `Boka: ${resourceType.name}`,
    resource: `Boka: ${resourceType.name}`,
    preview: `Boka: ${resourceType.name}`,
    confirmed: "Bokning bekräftad",
  };

  return (
    <Modal title={titles[step]} onClose={onClose} size="lg">
      {step === "time" && (
        <>
          {isAdmin && <UserTargetPicker targetUser={targetUser} onChange={setTargetUser} />}

          <p className="text-xs text-gray-500 dark:text-gray-400 mb-3">
            Välj datum och tid. Vilken {resourceType.name.toLowerCase()} du får väljer du i nästa
            steg.
          </p>

          <div className="rounded-lg border border-gray-200 dark:border-gray-800 p-3 mb-4">
            <div className="flex items-center justify-between mb-2">
              <button
                type="button"
                onClick={() => setViewMonth((m) => shiftMonth(m, -1))}
                disabled={!canGoPrevMonth}
                className="flex h-9 w-9 items-center justify-center rounded-md text-lg text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800 disabled:opacity-30 disabled:cursor-not-allowed"
              >
                ‹
              </button>
              <span className="text-sm font-medium capitalize text-gray-900 dark:text-gray-100">
                {monthLabel}
                {monthLoading && <span className="ml-2 text-gray-400 font-normal">Laddar...</span>}
              </span>
              <button
                type="button"
                onClick={() => setViewMonth((m) => shiftMonth(m, 1))}
                disabled={!canGoNextMonth}
                className="flex h-9 w-9 items-center justify-center rounded-md text-lg text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800 disabled:opacity-30 disabled:cursor-not-allowed"
              >
                ›
              </button>
            </div>

            <MonthGrid
              viewMonth={viewMonth}
              selectedDate={date}
              minDate={minDate}
              maxDate={maxDate}
              weekdayTextClassName="text-[11px]"
              availableDates={availableDates}
              onSelectDate={(iso) => {
                if (iso === date) return;
                setDate(iso);
                clear();
                setChosenResource(null);
                setError(null);
              }}
            />
          </div>

          <div className="mb-4 space-y-3">
            <DayNav
              dateLabel={formatDate(`${date}T00:00:00`)}
              onPrev={previousDay}
              onNext={nextDay}
              prevDisabled={isToday}
              nextDisabled={isMaxDate}
            />
            <div className="flex items-center justify-center gap-1.5 text-[11px] text-gray-400 dark:text-gray-500">
              <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />
              <span>Lediga tider ({resources.length} {resourceType.name.toLowerCase()})</span>
            </div>
            {maxDate && (
              <p className="text-center text-[11px] text-gray-400 dark:text-gray-500">
                Går att boka max {resourceType.maxAdvanceDays} dagar fram i tiden
              </p>
            )}
          </div>

          {isLoading && <p className="text-sm text-gray-500">Laddar lediga tider...</p>}
          {error && <p className="text-sm text-red-600 mb-3">{error}</p>}

          {!isLoading && slots.length === 0 && (
            <p className="text-sm text-gray-500">
              {closedForRestOfToday
                ? "Stängt för resten av idag. Prova ett annat datum."
                : "Inga lediga tider denna dag."}
            </p>
          )}

          {!isLoading && hasFullDayOption && (
            <label className="mb-3 flex items-center gap-2 text-sm text-gray-700 dark:text-gray-300 cursor-pointer select-none">
              <input
                type="checkbox"
                checked={isFullDaySelected}
                onChange={(e) => {
                  setError(null);
                  if (e.target.checked) selectFullDay();
                  else clear();
                }}
                className="h-4 w-4 rounded border-gray-300 dark:border-gray-700 text-indigo-600 focus:ring-indigo-500"
              />
              Boka hel dag
            </label>
          )}

          <SlotGrid
            slots={slots.map((s) => ({
              startUtc: s.startUtc,
              endUtc: s.endUtc,
              available: s.availableResourceIds.length > 0,
            }))}
            isSelected={isSelected}
            onToggle={(index) => {
              setError(null);
              toggleSlot(index);
            }}
          />

          {rangeStart && rangeEnd && (
            <div className="mt-4 flex items-center justify-between rounded-lg border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50 px-4 py-3">
              <div>
                <p className="text-sm font-medium text-gray-900 dark:text-gray-100">
                  {formatTime(rangeStart.startUtc)}–{formatTime(rangeEnd.endUtc)}
                </p>
                <p className="text-xs text-gray-500 dark:text-gray-400">
                  {duration} minuter (max {resourceType.maxDurationMinutes} min)
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
                  onClick={goToResourceStep}
                  disabled={exceedsMax || needsTargetUser}
                  className="rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50"
                >
                  Välj {resourceType.name.toLowerCase()}
                </button>
              </div>
            </div>
          )}
        </>
      )}

      {step === "resource" && rangeStart && rangeEnd && (
        <div className="space-y-4">
          <div className="flex items-center justify-between rounded-lg border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50 px-4 py-3">
            <div>
              <p className="text-sm font-medium text-gray-900 dark:text-gray-100">
                {formatDate(`${date}T00:00:00`)} · {formatTime(rangeStart.startUtc)}–
                {formatTime(rangeEnd.endUtc)}
              </p>
              <p className="text-xs text-gray-500 dark:text-gray-400">{duration} minuter</p>
            </div>
            <button
              onClick={() => setStep("time")}
              className="text-sm font-medium text-gray-500 hover:text-gray-900 dark:hover:text-gray-100"
            >
              Ändra tid
            </button>
          </div>

          <p className="text-sm font-medium text-gray-900 dark:text-gray-100">
            Välj {resourceType.name.toLowerCase()} ({candidateResources.length} lediga)
          </p>

          <div className="space-y-2 max-h-72 overflow-y-auto">
            {candidateResources.map((r) => (
              <button
                key={r.id}
                onClick={() => setChosenResource(r)}
                className={`w-full text-left rounded-lg border px-4 py-3 transition-colors ${
                  chosenResource?.id === r.id
                    ? "border-indigo-600 bg-indigo-50 dark:bg-indigo-900/30"
                    : "border-gray-200 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800/50"
                }`}
              >
                <p className="font-medium text-gray-900 dark:text-gray-100">{r.name}</p>
                {r.description && (
                  <p className="text-sm text-gray-500 dark:text-gray-400">{r.description}</p>
                )}
              </button>
            ))}
          </div>

          {error && <p className="text-sm text-red-600">{error}</p>}

          <div className="flex justify-end gap-2">
            <button
              onClick={() => setStep("time")}
              className="text-sm font-medium text-gray-500 hover:text-gray-900 dark:hover:text-gray-100"
            >
              Tillbaka
            </button>
            <button
              onClick={goToPreview}
              disabled={!chosenResource}
              className="rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50"
            >
              Granska bokning
            </button>
          </div>
        </div>
      )}

      {step === "preview" && rangeStart && rangeEnd && chosenResource && (
        <BookingPreviewStep
          resourceName={chosenResource.name}
          targetUserFullName={targetUserFullName}
          dateLabel={formatDate(`${date}T00:00:00`)}
          timeLabel={`${formatTime(rangeStart.startUtc)}–${formatTime(rangeEnd.endUtc)}`}
          durationMinutes={duration}
          error={error}
          isPending={bookMutation.isPending}
          onBack={() => setStep("resource")}
          onConfirm={handleConfirm}
        />
      )}

      {step === "confirmed" && rangeStart && rangeEnd && chosenResource && (
        <BookingConfirmedStep
          summary={`${chosenResource.name} · ${formatDate(`${date}T00:00:00`)} · ${formatTime(rangeStart.startUtc)}–${formatTime(rangeEnd.endUtc)}`}
          targetUserFullName={targetUserFullName}
          onBookAnother={bookAnother}
          onClose={onClose}
        />
      )}
    </Modal>
  );
}
