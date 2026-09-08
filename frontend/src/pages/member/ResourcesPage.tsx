import { useState } from "react";
import { StatusBadge } from "../../components/StatusBadge";
import { ResourceTypeFilter } from "../../components/ResourceTypeFilter";
import type { Resource } from "../../api/types";
import { BookingModal } from "./BookingModal";
import { useResources, useResourceTypes } from "../../hooks/useResources";
import {
  buildTypeNameMap,
  filterByResourceType,
  groupByTypeName,
} from "../../lib/resourceGrouping";
import { useResourceStatusUpdates } from "../../hooks/useResourceStatusUpdates";

export function ResourcesPage() {
  useResourceStatusUpdates();

  const [bookingResource, setBookingResource] = useState<Resource | null>(null);
  const [selectedTypeId, setSelectedTypeId] = useState<string | null>(null);

  const resourcesQuery = useResources();
  const typesQuery = useResourceTypes();

  const typeNameById = buildTypeNameMap(typesQuery.data ?? []);
  const visibleResources = filterByResourceType(
    resourcesQuery.data ?? [],
    selectedTypeId,
  );
  const grouped = groupByTypeName(visibleResources, typeNameById);

  return (
    <div>
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100 mb-4">
        Resurser
      </h1>

      <ResourceTypeFilter
        resourceTypes={typesQuery.data ?? []}
        selectedTypeId={selectedTypeId}
        onChange={setSelectedTypeId}
      />

      {resourcesQuery.isLoading && (
        <p className="text-gray-500">Laddar resurser...</p>
      )}
      {!resourcesQuery.isLoading && visibleResources.length === 0 && (
        <p className="text-gray-500">Inga resurser av den valda typen.</p>
      )}

      <div className="space-y-8">
        {Array.from(grouped.entries()).map(([typeName, resources]) => (
          <section key={typeName}>
            <h2 className="text-sm font-semibold uppercase tracking-wide text-gray-400 mb-3">
              {typeName}
            </h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
              {resources.map((resource) => (
                <div
                  key={resource.id}
                  className="rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-4 flex flex-col gap-2"
                >
                  <div className="flex items-start justify-between gap-2">
                    <h3 className="font-medium text-gray-900 dark:text-gray-100">
                      {resource.name}
                    </h3>
                    <StatusBadge status={resource.status} />
                  </div>
                  <p className="text-sm text-gray-500 dark:text-gray-400 flex-1">
                    {resource.description}
                  </p>
                  <button
                    disabled={resource.status !== "Online"}
                    onClick={() => setBookingResource(resource)}
                    className="mt-2 rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-40 disabled:cursor-not-allowed"
                  >
                    Boka
                  </button>
                </div>
              ))}
            </div>
          </section>
        ))}
      </div>

      {bookingResource && (
        <BookingModal
          resource={bookingResource}
          onClose={() => setBookingResource(null)}
        />
      )}
    </div>
  );
}
