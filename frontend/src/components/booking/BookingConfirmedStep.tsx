export function BookingConfirmedStep({
  summary,
  targetUserFullName,
  onBookAnother,
  onClose,
}: {
  summary: string;
  targetUserFullName?: string;
  onBookAnother: () => void;
  onClose: () => void;
}) {
  return (
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
        <p className="text-base font-semibold text-gray-900 dark:text-gray-100">Bokningen är klar</p>
        <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">{summary}</p>
        {targetUserFullName && (
          <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">
            Bokad åt {targetUserFullName}
          </p>
        )}
      </div>
      <div className="flex justify-center gap-2 pt-2">
        <button
          onClick={onBookAnother}
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
  );
}
