export function BookingPreviewStep({
  resourceName,
  targetUserFullName,
  dateLabel,
  timeLabel,
  durationMinutes,
  error,
  isPending,
  onBack,
  onConfirm,
}: {
  resourceName: string;
  targetUserFullName?: string;
  dateLabel: string;
  timeLabel: string;
  durationMinutes: number;
  error: string | null;
  isPending: boolean;
  onBack: () => void;
  onConfirm: () => void;
}) {
  return (
    <div className="space-y-4">
      <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50 p-4 space-y-2.5">
        <div className="flex justify-between text-sm">
          <span className="text-gray-500 dark:text-gray-400">Resurs</span>
          <span className="font-medium text-gray-900 dark:text-gray-100">{resourceName}</span>
        </div>
        {targetUserFullName && (
          <div className="flex justify-between text-sm">
            <span className="text-gray-500 dark:text-gray-400">Boka åt</span>
            <span className="font-medium text-gray-900 dark:text-gray-100">{targetUserFullName}</span>
          </div>
        )}
        <div className="flex justify-between text-sm">
          <span className="text-gray-500 dark:text-gray-400">Datum</span>
          <span className="font-medium text-gray-900 dark:text-gray-100">{dateLabel}</span>
        </div>
        <div className="flex justify-between text-sm">
          <span className="text-gray-500 dark:text-gray-400">Tid</span>
          <span className="font-medium text-gray-900 dark:text-gray-100">{timeLabel}</span>
        </div>
        <div className="flex justify-between text-sm">
          <span className="text-gray-500 dark:text-gray-400">Längd</span>
          <span className="font-medium text-gray-900 dark:text-gray-100">{durationMinutes} minuter</span>
        </div>
      </div>

      {error && <p className="text-sm text-red-600">{error}</p>}

      <div className="flex justify-end gap-2">
        <button
          onClick={onBack}
          disabled={isPending}
          className="text-sm font-medium text-gray-500 hover:text-gray-900 dark:hover:text-gray-100 disabled:opacity-50"
        >
          Tillbaka
        </button>
        <button
          onClick={onConfirm}
          disabled={isPending}
          className="rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50"
        >
          {isPending ? "Bokar..." : "Bekräfta bokning"}
        </button>
      </div>
    </div>
  );
}
