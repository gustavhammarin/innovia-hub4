import type { ResourceStatus } from "../api/types";

const STYLES: Record<ResourceStatus, string> = {
  Online: "bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-300",
  Maintenance: "bg-amber-100 text-amber-800 dark:bg-amber-900/40 dark:text-amber-300",
  Offline: "bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300",
  Archived: "bg-red-100 text-red-800 dark:bg-red-900/40 dark:text-red-300",
};

export function StatusBadge({ status }: { status: ResourceStatus }) {
  return (
    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${STYLES[status]}`}>
      {status}
    </span>
  );
}
