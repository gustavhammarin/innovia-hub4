import { buildMonthMatrix } from "../lib/calendarGrid";

const WEEKDAY_LABELS = ["Mån", "Tis", "Ons", "Tor", "Fre", "Lör", "Sön"];

export function MonthGrid({
  viewMonth,
  selectedDate,
  minDate,
  maxDate,
  cellMinHeight = "min-h-9",
  weekdayTextClassName = "text-xs",
  todayDate,
  availableDates,
  onSelectDate,
}: {
  viewMonth: string;
  selectedDate: string;
  minDate?: string;
  maxDate?: string;
  cellMinHeight?: string;
  weekdayTextClassName?: string;
  todayDate?: string;
  availableDates?: Set<string>;
  onSelectDate: (iso: string) => void;
}) {
  return (
    <>
      <div className={`grid grid-cols-7 gap-1 text-center ${weekdayTextClassName} text-gray-400 mb-1`}>
        {WEEKDAY_LABELS.map((label) => (
          <span key={label}>{label}</span>
        ))}
      </div>

      <div className="grid grid-cols-7 gap-1">
        {buildMonthMatrix(`${viewMonth}-01`).map((cell) => {
          const disabled = (!!minDate && cell.iso < minDate) || (!!maxDate && cell.iso > maxDate);
          const selected = cell.iso === selectedDate;
          const isToday = todayDate !== undefined && cell.iso === todayDate;
          const hasAvailability = !disabled && !!availableDates?.has(cell.iso);
          return (
            <button
              key={cell.iso}
              type="button"
              disabled={disabled}
              onClick={() => onSelectDate(cell.iso)}
              className={`relative aspect-square ${cellMinHeight} rounded-md text-sm font-medium ${
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
    </>
  );
}
