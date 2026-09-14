import { useState } from "react";
import { Modal } from "../../components/Modal";
import { DateField } from "../../components/DateField";
import { ApiError } from "../../api/client";
import type { AppUser, Resource } from "../../api/types";
import { useResourceAvailability } from "../../hooks/useBookings";
import { useCreateBooking } from "../../hooks/useBookingMutations";
import { useResourceTypes } from "../../hooks/useResources";
import { useUserSearch } from "../../hooks/useUsers";
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
  const [userSearch, setUserSearch] = useState("");
  const [userSearchOpen, setUserSearchOpen] = useState(false);
  const userSearchQuery = useUserSearch(userSearch, isAdmin && userSearchOpen);

  const { data, isLoading } = useResourceAvailability(resource.id, date, date);

  const slotsByDate = groupSlotsByDate(data?.slots ?? []);
  const slots = slotsByDate.get(date) ?? [];
  const { toggleSlot, clear, isSelected, rangeStart, rangeEnd, duration, exceedsMax } =
    useSlotSelection(slotsByDate, maxDurationMinutes);

  const bookMutation = useCreateBooking();
  useResourceBookingUpdates(resource.id);

  const needsTargetUser = isAdmin && !targetUser;

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
                    className="w-full rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-base sm:text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
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
            Klicka en starttid, klicka sedan en sluttid för att boka flera timmar i följd.
          </p>

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

          <div className="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-5 gap-2">
            {slots.map((slot, index) => {
              const selected = isSelected(date, index);
              return (
                <button
                  key={slot.startUtc}
                  disabled={!slot.isAvailable}
                  onClick={() => {
                    setError(null);
                    toggleSlot(date, index);
                  }}
                  className={`rounded-md px-1.5 py-2 text-xs sm:text-sm font-medium border transition-all ${
                    selected
                      ? "border-indigo-600 bg-indigo-600 text-white shadow-sm"
                      : slot.isAvailable
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
        <div className="space-y-4">
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50 p-4 space-y-2.5">
            <div className="flex justify-between text-sm">
              <span className="text-gray-500 dark:text-gray-400">Resurs</span>
              <span className="font-medium text-gray-900 dark:text-gray-100">{resource.name}</span>
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
              onClick={() => setStep("select")}
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

      {step === "confirmed" && rangeStart && rangeEnd && (
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
              {resource.name} · {formatDate(`${date}T00:00:00`)} · {formatTime(rangeStart.startUtc)}–
              {formatTime(rangeEnd.endUtc)}
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
