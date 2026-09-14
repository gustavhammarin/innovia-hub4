import { useState } from "react";
import { buildMonthMatrix, dateToIso, shiftMonth } from "../lib/calendarGrid";

const WEEKDAY_LABELS = ["Mån", "Tis", "Ons", "Tor", "Fre", "Lör", "Sön"];

function formatDisplay(iso: string): string {
  return new Date(`${iso}T00:00:00`).toLocaleDateString("sv-SE", {
    day: "numeric",
    month: "short",
    year: "numeric",
  });
}

export function MobileDatePicker({
  label,
  value,
  onChange,
  minDate,
  maxDate,
}: {
  label: string;
  value: string;
  onChange: (iso: string) => void;
  minDate?: string;
  maxDate?: string;
}) {
  const [open, setOpen] = useState(false);
  const [visible, setVisible] = useState(false);
  const [draft, setDraft] = useState(value);
  const [viewDate, setViewDate] = useState(value || dateToIso(new Date()));

  function openSheet() {
    setDraft(value);
    setViewDate(value || dateToIso(new Date()));
    setOpen(true);
    requestAnimationFrame(() => requestAnimationFrame(() => setVisible(true)));
  }

  function closeSheet() {
    setVisible(false);
    setTimeout(() => setOpen(false), 200);
  }

  function confirm() {
    onChange(draft);
    closeSheet();
  }

  const today = dateToIso(new Date());
  const viewYm = viewDate.slice(0, 7);
  const canGoPrevMonth = !minDate || viewYm > minDate.slice(0, 7);
  const canGoNextMonth = !maxDate || viewYm < maxDate.slice(0, 7);
  const monthLabel = new Date(`${viewYm}-01T00:00:00`).toLocaleDateString("sv-SE", {
    month: "long",
    year: "numeric",
  });

  return (
    <div className="flex-1 min-w-0">
      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
        {label}
      </label>
      <button
        type="button"
        onClick={openSheet}
        className="w-full min-h-11 rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2.5 text-left text-base sm:text-sm text-gray-900 dark:text-gray-100 truncate"
      >
        {value ? formatDisplay(value) : <span className="text-gray-400">Välj datum</span>}
      </button>

      {open && (
        <div
          className="fixed inset-0 z-50 flex items-end justify-center sm:items-center bg-black/50"
          style={{ opacity: visible ? 1 : 0, transition: "opacity 200ms" }}
          onClick={closeSheet}
        >
          <div
            className="w-full sm:max-w-sm rounded-t-2xl sm:rounded-2xl bg-white dark:bg-gray-900 p-4"
            style={{
              transform: visible ? "translateY(0)" : "translateY(100%)",
              transition: "transform 200ms ease-out",
              paddingBottom: "max(1rem, env(safe-area-inset-bottom))",
            }}
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center justify-between mb-3">
              <button
                type="button"
                onClick={() => setViewDate((d) => shiftMonth(d, -1))}
                disabled={!canGoPrevMonth}
                className="flex h-10 w-10 items-center justify-center rounded-md text-lg text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800 disabled:opacity-30 disabled:cursor-not-allowed"
              >
                ‹
              </button>
              <span className="text-base font-medium capitalize text-gray-900 dark:text-gray-100">
                {monthLabel}
              </span>
              <button
                type="button"
                onClick={() => setViewDate((d) => shiftMonth(d, 1))}
                disabled={!canGoNextMonth}
                className="flex h-10 w-10 items-center justify-center rounded-md text-lg text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800 disabled:opacity-30 disabled:cursor-not-allowed"
              >
                ›
              </button>
            </div>

            <div className="grid grid-cols-7 gap-1 text-center text-xs text-gray-400 mb-1">
              {WEEKDAY_LABELS.map((l) => (
                <span key={l}>{l}</span>
              ))}
            </div>

            <div className="grid grid-cols-7 gap-1">
              {buildMonthMatrix(viewDate).map((cell) => {
                const disabled =
                  (!!minDate && cell.iso < minDate) || (!!maxDate && cell.iso > maxDate);
                const selected = cell.iso === draft;
                const isToday = cell.iso === today;
                return (
                  <button
                    key={cell.iso}
                    type="button"
                    disabled={disabled}
                    onClick={() => setDraft(cell.iso)}
                    className={`aspect-square min-h-11 rounded-md text-sm font-medium ${
                      !cell.inMonth ? "text-gray-300 dark:text-gray-700" : "text-gray-700 dark:text-gray-300"
                    } ${
                      selected
                        ? "bg-indigo-600 text-white"
                        : disabled
                          ? "cursor-not-allowed opacity-30"
                          : "hover:bg-indigo-50 dark:hover:bg-indigo-900/30"
                    } ${isToday && !selected ? "ring-1 ring-inset ring-indigo-400" : ""}`}
                  >
                    {cell.dayNum}
                  </button>
                );
              })}
            </div>

            <div className="mt-4 flex gap-2">
              <button
                type="button"
                onClick={closeSheet}
                className="flex-1 rounded-md px-3 py-2.5 text-sm font-medium text-gray-600 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-800"
              >
                Avbryt
              </button>
              <button
                type="button"
                onClick={confirm}
                className="flex-1 rounded-md bg-indigo-600 px-3 py-2.5 text-sm font-medium text-white hover:bg-indigo-500"
              >
                Klar
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
