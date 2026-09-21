import { formatTime } from "../../lib/date";

export interface SlotGridItem {
  startUtc: string;
  endUtc: string;
  available: boolean;
}

export function SlotGrid({
  slots,
  isSelected,
  onToggle,
}: {
  slots: SlotGridItem[];
  isSelected: (index: number) => boolean;
  onToggle: (index: number) => void;
}) {
  return (
    <div className="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-5 gap-2">
      {slots.map((slot, index) => {
        const selected = isSelected(index);
        return (
          <button
            key={slot.startUtc}
            disabled={!slot.available}
            onClick={() => onToggle(index)}
            className={`rounded-md px-1.5 py-2 text-xs sm:text-sm font-medium border transition-all ${
              selected
                ? "border-indigo-600 bg-indigo-600 text-white shadow-sm"
                : slot.available
                  ? "border-indigo-200 text-indigo-700 hover:bg-indigo-50 dark:border-indigo-800 dark:text-indigo-300 dark:hover:bg-indigo-900/30"
                  : "border-gray-100 text-gray-300 line-through dark:border-gray-800 dark:text-gray-600 cursor-not-allowed"
            }`}
          >
            {formatTime(slot.startUtc)}–{formatTime(slot.endUtc)}
          </button>
        );
      })}
    </div>
  );
}
