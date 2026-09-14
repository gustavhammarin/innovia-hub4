import { useEffect, useState } from "react";
import { Modal } from "../../components/Modal";
import { ApiError } from "../../api/client";
import type { AppUser, Resource, ResourceType } from "../../api/types";
import { useResourcesAvailability } from "../../hooks/useResourcesAvailability";
import { useCreateBooking } from "../../hooks/useBookingMutations";
import { useUserSearch } from "../../hooks/useUsers";
import { useDayRange } from "../../hooks/useDayRange";
import { useAggregatedSlotSelection } from "../../hooks/useAggregatedSlotSelection";
import { aggregateSlotsAcrossResources, datesWithAvailability } from "../../lib/availability";
import { formatDate, formatTime } from "../../lib/date";
import { buildMonthMatrix, monthAvailabilityRange, shiftMonth } from "../../lib/calendarGrid";
import { useResourcesBookingUpdates } from "../../hooks/useResourceBookingUpdates";
import { useAuth } from "../../auth/AuthContext";

const WEEKDAY_LABELS = ["Mån", "Tis", "Ons", "Tor", "Fre", "Lör", "Sön"];

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
  const [userSearch, setUserSearch] = useState("");
  const [userSearchOpen, setUserSearchOpen] = useState(false);
  const userSearchQuery = useUserSearch(userSearch, isAdmin && userSearchOpen);

  const resourceIds = resources.map((r) => r.id);
  const { byResourceId, isLoading } = useResourcesAvailability(resourceIds, date, date);
  const slots = aggregateSlotsAcrossResources(resourceIds, byResourceId);

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
          {isAdmin && (
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Boka åt användare
              </label>
              {targetUser ? (
                <div className="flex items-center justify-between rounded-md border border-indigo-200 dark:border-indigo-800 bg-indigo-50 dark:bg-indigo-900/30 px-3 py-2">
                  <div>
                    <p className="text-sm font-medium text-gray-900 dark:text-gray-100">
                      {targetUser.fullName}
                    </p>
                    <p className="text-xs text-gray-500 dark:text-gray-400">{targetUser.email}</p>
                  </div>
                  <button
                    onClick={() => setTargetUser(null)}
                    className="text-sm text-gray-500 hover:text-gray-900 dark:hover:text-gray-100"
                  >
                    Byt
                  </button>
                </div>
              ) : (
                <div className="relative">
                  <input
                    type="text"
                    value={userSearch}
                    onChange={(e) => {
                      setUserSearch(e.target.value);
                      setUserSearchOpen(true);
                    }}
                    onFocus={() => setUserSearchOpen(true)}
                    placeholder="Sök på namn eller e-post..."
                    className="w-full rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  />
                  {userSearchOpen && (
                    <div className="absolute z-10 mt-1 w-full rounded-md border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-lg max-h-48 overflow-y-auto">
                      {userSearchQuery.isLoading && (
                        <p className="px-3 py-2 text-sm text-gray-500">Söker...</p>
                      )}
                      {!userSearchQuery.isLoading && userSearchQuery.data?.length === 0 && (
                        <p className="px-3 py-2 text-sm text-gray-500">Ingen användare hittades.</p>
                      )}
                      {userSearchQuery.data?.map((u) => (
                        <button
                          key={u.id}
                          onClick={() => {
                            setTargetUser(u);
                            setUserSearchOpen(false);
                            setUserSearch("");
                          }}
                          className="block w-full text-left px-3 py-2 text-sm hover:bg-gray-50 dark:hover:bg-gray-800"
                        >
                          <span className="font-medium text-gray-900 dark:text-gray-100">
                            {u.fullName}
                          </span>{" "}
                          <span className="text-gray-500 dark:text-gray-400">{u.email}</span>
                        </button>
                      ))}
                    </div>
                  )}
                </div>
              )}
            </div>
          )}

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

            <div className="grid grid-cols-7 gap-1 text-center text-[11px] text-gray-400 mb-1">
              {WEEKDAY_LABELS.map((label) => (
                <span key={label}>{label}</span>
              ))}
            </div>

            <div className="grid grid-cols-7 gap-1">
              {buildMonthMatrix(`${viewMonth}-01`).map((cell) => {
                const disabled = cell.iso < minDate || (!!maxDate && cell.iso > maxDate);
                const selected = cell.iso === date;
                const hasAvailability = !disabled && availableDates.has(cell.iso);
                return (
                  <button
                    key={cell.iso}
                    type="button"
                    disabled={disabled}
                    onClick={() => {
                      if (cell.iso === date) return;
                      setDate(cell.iso);
                      clear();
                      setChosenResource(null);
                      setError(null);
                    }}
                    className={`relative aspect-square min-h-9 rounded-md text-sm font-medium ${
                      !cell.inMonth ? "text-gray-300 dark:text-gray-700" : "text-gray-700 dark:text-gray-300"
                    } ${
                      selected
                        ? "bg-indigo-600 text-white"
                        : disabled
                          ? "cursor-not-allowed opacity-30"
                          : hasAvailability
                            ? "bg-emerald-50 hover:bg-emerald-100 dark:bg-emerald-900/20 dark:hover:bg-emerald-900/40"
                            : "hover:bg-indigo-50 dark:hover:bg-indigo-900/30"
                    }`}
                  >
                    {cell.dayNum}
                    {hasAvailability && !selected && (
                      <span className="absolute bottom-1 left-1/2 h-1 w-1 -translate-x-1/2 rounded-full bg-emerald-500" />
                    )}
                  </button>
                );
              })}
            </div>
          </div>

          <div className="mb-4 space-y-3">
            <div className="flex items-center justify-between">
              <span className="text-sm font-semibold text-gray-900 dark:text-gray-100">
                {formatDate(`${date}T00:00:00`)}
              </span>
              <div className="flex items-center gap-3">
                <button
                  onClick={previousDay}
                  disabled={isToday}
                  className="text-sm text-gray-500 hover:text-gray-900 dark:hover:text-gray-100 disabled:opacity-30 disabled:cursor-not-allowed"
                >
                  ← Dag
                </button>
                <button
                  onClick={nextDay}
                  disabled={isMaxDate}
                  className="text-sm text-gray-500 hover:text-gray-900 dark:hover:text-gray-100 disabled:opacity-30 disabled:cursor-not-allowed"
                >
                  Dag →
                </button>
              </div>
            </div>
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
            <p className="text-sm text-gray-500">Inga lediga tider denna dag.</p>
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

          <div className="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-5 gap-2">
            {slots.map((slot, index) => {
              const selected = isSelected(index);
              const available = slot.availableResourceIds.length > 0;
              return (
                <button
                  key={slot.startUtc}
                  disabled={!available}
                  onClick={() => {
                    setError(null);
                    toggleSlot(index);
                  }}
                  className={`rounded-md px-1.5 py-2 text-xs sm:text-sm font-medium border transition-all ${
                    selected
                      ? "border-indigo-600 bg-indigo-600 text-white shadow-sm"
                      : available
                        ? "border-indigo-200 text-indigo-700 hover:bg-indigo-50 dark:border-indigo-800 dark:text-indigo-300 dark:hover:bg-indigo-900/30"
                        : "border-gray-100 text-gray-300 line-through dark:border-gray-800 dark:text-gray-600 cursor-not-allowed"
                  }`}
                >
                  {formatTime(slot.startUtc)}–{formatTime(slot.endUtc)}
                </button>
              );
            })}
          </div>

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
        <div className="space-y-4">
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50 p-4 space-y-2.5">
            <div className="flex justify-between text-sm">
              <span className="text-gray-500 dark:text-gray-400">Resurs</span>
              <span className="font-medium text-gray-900 dark:text-gray-100">
                {chosenResource.name}
              </span>
            </div>
            {isAdmin && targetUser && (
              <div className="flex justify-between text-sm">
                <span className="text-gray-500 dark:text-gray-400">Boka åt</span>
                <span className="font-medium text-gray-900 dark:text-gray-100">
                  {targetUser.fullName}
                </span>
              </div>
            )}
            <div className="flex justify-between text-sm">
              <span className="text-gray-500 dark:text-gray-400">Datum</span>
              <span className="font-medium text-gray-900 dark:text-gray-100">
                {formatDate(`${date}T00:00:00`)}
              </span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-gray-500 dark:text-gray-400">Tid</span>
              <span className="font-medium text-gray-900 dark:text-gray-100">
                {formatTime(rangeStart.startUtc)}–{formatTime(rangeEnd.endUtc)}
              </span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-gray-500 dark:text-gray-400">Längd</span>
              <span className="font-medium text-gray-900 dark:text-gray-100">{duration} minuter</span>
            </div>
          </div>

          {error && <p className="text-sm text-red-600">{error}</p>}

          <div className="flex justify-end gap-2">
            <button
              onClick={() => setStep("resource")}
              disabled={bookMutation.isPending}
              className="text-sm font-medium text-gray-500 hover:text-gray-900 dark:hover:text-gray-100 disabled:opacity-50"
            >
              Tillbaka
            </button>
            <button
              onClick={handleConfirm}
              disabled={bookMutation.isPending}
              className="rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50"
            >
              {bookMutation.isPending ? "Bokar..." : "Bekräfta bokning"}
            </button>
          </div>
        </div>
      )}

      {step === "confirmed" && rangeStart && rangeEnd && chosenResource && (
        <div className="space-y-4 py-2 text-center">
          <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-green-100 dark:bg-green-900/30">
            <svg
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth={2.5}
              strokeLinecap="round"
              strokeLinejoin="round"
              className="h-6 w-6 text-green-600 dark:text-green-400"
            >
              <path d="M5 13l4 4L19 7" />
            </svg>
          </div>
          <div>
            <p className="text-base font-semibold text-gray-900 dark:text-gray-100">
              Bokningen är klar
            </p>
            <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
              {chosenResource.name} · {formatDate(`${date}T00:00:00`)} ·{" "}
              {formatTime(rangeStart.startUtc)}–{formatTime(rangeEnd.endUtc)}
            </p>
            {isAdmin && targetUser && (
              <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">
                Bokad åt {targetUser.fullName}
              </p>
            )}
          </div>
          <div className="flex justify-center gap-2 pt-2">
            <button
              onClick={bookAnother}
              className="text-sm font-medium text-gray-500 hover:text-gray-900 dark:hover:text-gray-100"
            >
              Boka en till
            </button>
            <button
              onClick={onClose}
              className="rounded-md bg-indigo-600 px-4 py-1.5 text-sm font-medium text-white hover:bg-indigo-500"
            >
              Stäng
            </button>
          </div>
        </div>
      )}
    </Modal>
  );
}
