import { useState } from "react";
import type { ResourceType } from "../../api/types";
import { useResourceTypes } from "../../hooks/useResources";
import { ResourceTypeFormModal } from "./ResourceTypeFormModal";

export function ResourceTypesAdminPage() {
  const [editing, setEditing] = useState<ResourceType | null>(null);
  const [creating, setCreating] = useState(false);

  const typesQuery = useResourceTypes();

  return (
    <div>
      <div className="sticky top-14 z-10 -mx-4 px-4 bg-gray-50 dark:bg-gray-950 pt-1 pb-3 mb-1">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">
            Resurstyper
          </h1>
          <button
            onClick={() => setCreating(true)}
            className="rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-indigo-500"
          >
            + Ny resurstyp
          </button>
        </div>
      </div>

      <div className="hidden md:block overflow-x-auto rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900">
        <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-800 text-sm">
          <thead>
            <tr className="text-left text-xs font-semibold uppercase text-gray-400">
              <th className="px-4 py-3">Namn</th>
              <th className="px-4 py-3">Max längd (min)</th>
              <th className="px-4 py-3">Max förhandsbokning (dagar)</th>
              <th className="px-4 py-3 text-right">Åtgärder</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
            {(typesQuery.data ?? []).map((type) => (
              <tr key={type.id}>
                <td className="px-4 py-3 font-medium text-gray-900 dark:text-gray-100">{type.name}</td>
                <td className="px-4 py-3 text-gray-600 dark:text-gray-300">{type.maxDurationMinutes}</td>
                <td className="px-4 py-3 text-gray-600 dark:text-gray-300">{type.maxAdvanceDays}</td>
                <td className="px-4 py-3 text-right">
                  <button
                    onClick={() => setEditing(type)}
                    className="text-indigo-600 hover:text-indigo-500 font-medium"
                  >
                    Redigera
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {(typesQuery.data ?? []).length === 0 && !typesQuery.isLoading && (
          <p className="text-sm text-gray-500 px-4 py-6">Inga resurstyper finns.</p>
        )}
      </div>

      <div className="md:hidden space-y-3">
        {(typesQuery.data ?? []).map((type) => (
          <div
            key={type.id}
            className="rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-4"
          >
            <div className="flex items-start justify-between gap-2">
              <div className="font-medium text-gray-900 dark:text-gray-100">
                {type.name}
              </div>
              <button
                onClick={() => setEditing(type)}
                className="shrink-0 text-sm text-indigo-600 hover:text-indigo-500 font-medium"
              >
                Redigera
              </button>
            </div>
            <div className="mt-1 text-sm text-gray-600 dark:text-gray-300">
              Max längd: {type.maxDurationMinutes} min
            </div>
            <div className="text-sm text-gray-600 dark:text-gray-300">
              Max förhandsbokning: {type.maxAdvanceDays} dagar
            </div>
          </div>
        ))}
        {(typesQuery.data ?? []).length === 0 && !typesQuery.isLoading && (
          <p className="text-sm text-gray-500 px-1 py-6">Inga resurstyper finns.</p>
        )}
      </div>

      {(creating || editing) && (
        <ResourceTypeFormModal
          resourceType={editing}
          onClose={() => {
            setCreating(false);
            setEditing(null);
          }}
        />
      )}
    </div>
  );
}
