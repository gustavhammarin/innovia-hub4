export function DayNav({
  dateLabel,
  onPrev,
  onNext,
  prevDisabled,
  nextDisabled,
}: {
  dateLabel: string;
  onPrev: () => void;
  onNext: () => void;
  prevDisabled: boolean;
  nextDisabled: boolean;
}) {
  return (
    <div className="flex items-center justify-between">
      <span className="text-sm font-semibold text-gray-900 dark:text-gray-100">{dateLabel}</span>
      <div className="flex items-center gap-3">
        <button
          onClick={onPrev}
          disabled={prevDisabled}
          className="text-sm text-gray-500 hover:text-gray-900 dark:hover:text-gray-100 disabled:opacity-30 disabled:cursor-not-allowed"
        >
          ← Dag
        </button>
        <button
          onClick={onNext}
          disabled={nextDisabled}
          className="text-sm text-gray-500 hover:text-gray-900 dark:hover:text-gray-100 disabled:opacity-30 disabled:cursor-not-allowed"
        >
          Dag →
        </button>
      </div>
    </div>
  );
}
