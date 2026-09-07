import { useState } from "react";
import { StatusBadge } from "../../components/StatusBadge";
import { ResourceTypeFilter } from "../../components/ResourceTypeFilter";
import { ApiError } from "../../api/client";
import type { Resource } from "../../api/types";
import { ResourceFormModal } from "./ResourceFormModal";
import { useResourcesAdmin, useResourceTypes } from "../../hooks/useResources";
import { useResourceStatusMutation, type ResourceStatusAction } from "../../hooks/useResourceMutations";
import { buildTypeNameMap, filterByResourceType } from "../../lib/resourceGrouping";

export function ResourcesAdminPage() {
  const [editing, setEditing] = useState<Resource | null>(null);
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedTypeId, setSelectedTypeId] = useState<string | null>(null);

  const resourcesQuery = useResourcesAdmin();
  const typesQuery = useResourceTypes();
  const typeNameById = buildTypeNameMap(typesQuery.data ?? []);

  const actionMutation = useResourceStatusMutation();

  function runAction(id: string, action: ResourceStatusAction) {
    setError(null);
    actionMutation.mutate(
      { id, action },
      { onError: (err) => setError(err instanceof ApiError ? err.message : "Något gick fel") }
    );
  }

  const resources = filterByResourceType(resourcesQuery.data ?? [], selectedTypeId);

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">Resurser (admin)</h1>
        <button
          onClick={() => setCreating(true)}
          className="rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-indigo-500"
        >
          + Ny resurs
        </button>
      </div>

      <ResourceTypeFilter
        resourceTypes={typesQuery.data ?? []}
        selectedTypeId={selectedTypeId}
        onChange={setSelectedTypeId}
      />

      {error && <p className="text-sm text-red-600 mb-4">{error}</p>}
      {resourcesQuery.isLoading && <p className="text-gray-500">Laddar...</p>}

      <div className="overflow-x-auto rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900">
        <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-800 text-sm">
          <thead>
            <tr className="text-left text-xs font-semibold uppercase text-gray-400">
              <th className="px-4 py-3">Namn</th>
              <th className="px-4 py-3">Typ</th>
              <th className="px-4 py-3">Status</th>
              <th className="px-4 py-3 text-right">Åtgärder</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
            {resources.map((resource) => (
              <tr key={resource.id}>
                <td className="px-4 py-3">
                  <div className="font-medium text-gray-900 dark:text-gray-100">{resource.name}</div>
                  <div className="text-gray-500 dark:text-gray-400">{resource.description}</div>
                </td>
                <td className="px-4 py-3 text-gray-600 dark:text-gray-300">
                  {typeNameById.get(resource.resourceTypeId) ?? "-"}
                </td>
                <td className="px-4 py-3">
                  <StatusBadge status={resource.status} />
                </td>
                <td className="px-4 py-3">
                  <div className="flex justify-end gap-2 flex-wrap">
                    <button
                      onClick={() => setEditing(resource)}
                      className="text-indigo-600 hover:text-indigo-500 font-medium"
                    >
                      Redigera
                    </button>
                    {resource.status !== "Online" && resource.status !== "Archived" && (
                      <button
                        onClick={() => runAction(resource.id, "online")}
                        className="text-green-600 hover:text-green-500 font-medium"
                      >
                        Online
                      </button>
                    )}
                    {resource.status !== "Maintenance" && resource.status !== "Archived" && (
                      <button
                        onClick={() => runAction(resource.id, "maintenance")}
                        className="text-amber-600 hover:text-amber-500 font-medium"
                      >
                        Underhåll
                      </button>
                    )}
                    {resource.status !== "Offline" && resource.status !== "Archived" && (
                      <button
                        onClick={() => runAction(resource.id, "offline")}
                        className="text-gray-500 hover:text-gray-700 font-medium"
                      >
                        Offline
                      </button>
                    )}
                    {resource.status === "Archived" ? (
                      <button
                        onClick={() => runAction(resource.id, "unarchive")}
                        className="text-indigo-600 hover:text-indigo-500 font-medium"
                      >
                        Återställ
                      </button>
                    ) : (
                      <button
                        onClick={() => runAction(resource.id, "archive")}
                        className="text-red-600 hover:text-red-500 font-medium"
                      >
                        Arkivera
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {resources.length === 0 && !resourcesQuery.isLoading && (
          <p className="text-sm text-gray-500 px-4 py-6">Inga resurser av den valda typen.</p>
        )}
      </div>

      {(creating || editing) && (
        <ResourceFormModal
          resource={editing}
          resourceTypes={typesQuery.data ?? []}
          onClose={() => {
            setCreating(false);
            setEditing(null);
          }}
        />
      )}
    </div>
  );
}
