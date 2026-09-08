import { useState } from "react";
import { todayIso } from "../lib/date";
import { groupSlotsByDate } from "../lib/availability";
import { useResourceAvailability } from "../hooks/useBookings";

const WEEKDAY_LABELS = ["Mån", "Tis", "Ons", "Tor", "Fre", "Lör", "Sön"];

function dateToIso(d: Date): string {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${y}-${m}-${day}`;
}

function shiftMonth(iso: string, delta: number): string {
  const [y, m] = iso.split("-").map(Number);
  return dateToIso(new Date(y, m - 1 + delta, 1));
}

function buildMonthMatrix(iso: string): { iso: string; dayNum: number; inMonth: boolean }[] {
  const [y, m] = iso.split("-").map(Number);
  const month = m - 1;
  const startOffset = (new Date(y, month, 1).getDay() + 6) % 7;

  return Array.from({ length: 42 }, (_, i) => {
    const cellDate = new Date(y, month, i - startOffset + 1);
    return {
      iso: dateToIso(cellDate),
      dayNum: cellDate.getDate(),
      inMonth: cellDate.getMonth() === month,
    };
  });
}

// The availability endpoint caps ranges at 31 days, so a single calendar month always fits.
function monthAvailabilityRange(viewYm: string, minDate: string, maxDate?: string) {
  const [y, m] = viewYm.split("-").map(Number);
  const first = dateToIso(new Date(y, m - 1, 1));
  const last = dateToIso(new Date(y, m, 0));
  const from = first < minDate ? minDate : first;
  const to = maxDate && last > maxDate ? maxDate : last;
  return { from, to };
}

function CalendarMonth({
  initialViewDate,
  value,
  minDate,
  maxDate,
  onChange,
  resourceId,
}: {
  initialViewDate: string;
  value: string;
  minDate: string;
  maxDate?: string;
  onChange: (iso: string) => void;
  resourceId: string;
}) {
  const [viewDate, setViewDate] = useState(initialViewDate);
  const today = todayIso();

  const viewYm = viewDate.slice(0, 7);
  const canGoPrevMonth = viewYm > minDate.slice(0, 7);
  const canGoNextMonth = !maxDate || viewYm < maxDate.slice(0, 7);

  const { from, to } = monthAvailabilityRange(viewYm, minDate, maxDate);
  const { data } = useResourceAvailability(resourceId, from, to);
  const slotsByDate = groupSlotsByDate(data?.slots ?? []);

  const monthLabel = new Date(`${viewYm}-01T00:00:00`).toLocaleDateString("sv-SE", {
    month: "long",
    year: "numeric",
  });

  return (
    <div className="rounded-lg border border-gray-200 dark:border-gray-800 p-3">
      <div className="flex items-center justify-between mb-2">
        <button
          type="button"
          onClick={() => setViewDate((d) => shiftMonth(d, -1))}
          disabled={!canGoPrevMonth}
          className="flex h-9 w-9 items-center justify-center rounded-md text-lg text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800 disabled:opacity-30 disabled:cursor-not-allowed"
        >
          ‹
        </button>
        <span className="text-sm font-medium capitalize text-gray-900 dark:text-gray-100">
          {monthLabel}
        </span>
        <button
          type="button"
          onClick={() => setViewDate((d) => shiftMonth(d, 1))}
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
        {buildMonthMatrix(viewDate).map((cell) => {
          const disabled = cell.iso < minDate || (!!maxDate && cell.iso > maxDate);
          const selected = cell.iso === value;
          const isToday = cell.iso === today;
          const hasAvailability =
            !disabled && (slotsByDate.get(cell.iso)?.some((s) => s.isAvailable) ?? false);
          return (
            <button
              key={cell.iso}
              type="button"
              disabled={disabled}
              onClick={() => onChange(cell.iso)}
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
              } ${isToday && !selected ? "ring-1 ring-inset ring-indigo-400" : ""}`}
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
  );
}

export function DateField({
  value,
  minDate,
  maxDate,
  onChange,
  resourceId,
}: {
  value: string;
  minDate: string;
  maxDate?: string;
  onChange: (iso: string) => void;
  resourceId: string;
}) {
  return (
    <CalendarMonth
      key={value.slice(0, 7)}
      initialViewDate={value}
      value={value}
      minDate={minDate}
      maxDate={maxDate}
      onChange={onChange}
      resourceId={resourceId}
    />
  );
}
