import { useEffect, useRef, useState, type ReactNode } from "react";
import { StatusBadge } from "../../components/StatusBadge";
import { ResourceTypeFilter } from "../../components/ResourceTypeFilter";
import { ApiError } from "../../api/client";
import type { Resource } from "../../api/types";
import { ResourceFormModal } from "./ResourceFormModal";
import { BookingModal } from "../member/BookingModal";
import { useResourcesAdmin, useResourceTypes } from "../../hooks/useResources";
import {
  useResourceStatusMutation,
  type ResourceStatusAction,
} from "../../hooks/useResourceMutations";
import {
  buildTypeNameMap,
  filterByResourceType,
} from "../../lib/resourceGrouping";
import { useResourceStatusUpdates } from "../../hooks/useResourceStatusUpdates";

function MenuItem({
  onClick,
  className,
  children,
}: {
  onClick: () => void;
  className?: string;
  children: ReactNode;
}) {
  return (
    <button
      onClick={onClick}
      className={`block w-full px-3 py-2 text-left text-sm font-medium hover:bg-gray-100 dark:hover:bg-gray-800 disabled:opacity-40 disabled:cursor-not-allowed ${className ?? "text-gray-700 dark:text-gray-200"}`}
    >
      {children}
    </button>
  );
}

function ResourceActions({
  resource,
  onBook,
  onEdit,
  onAction,
  align = "right",
}: {
  resource: Resource;
  onBook: () => void;
  onEdit: () => void;
  onAction: (action: ResourceStatusAction) => void;
  align?: "left" | "right";
}) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    function onClickOutside(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener("mousedown", onClickOutside);
    return () => document.removeEventListener("mousedown", onClickOutside);
  }, [open]);

  function run(fn: () => void) {
    fn();
    setOpen(false);
  }

  return (
    <div ref={ref} className="relative inline-block text-left">
      <button
        onClick={() => setOpen((v) => !v)}
        aria-expanded={open}
        className="rounded-md border border-gray-300 dark:border-gray-700 px-3 py-1.5 text-sm font-medium text-gray-700 dark:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800"
      >
        Hantera ▾
      </button>
      {open && (
        <div
          className={`absolute z-10 mt-1 w-44 rounded-md border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-lg py-1 ${
            align === "right" ? "right-0" : "left-0"
          }`}
        >
          <MenuItem
            onClick={() => run(onBook)}
            className={
              resource.status !== "Online"
                ? "text-gray-300 dark:text-gray-600 cursor-not-allowed pointer-events-none"
                : "text-indigo-600 dark:text-indigo-400"
            }
          >
            Boka
          </MenuItem>
          <MenuItem onClick={() => run(onEdit)} className="text-indigo-600 dark:text-indigo-400">
            Redigera
          </MenuItem>
          {resource.status !== "Online" && resource.status !== "Archived" && (
            <MenuItem onClick={() => run(() => onAction("online"))} className="text-green-600 dark:text-green-400">
              Online
            </MenuItem>
          )}
          {resource.status !== "Maintenance" && resource.status !== "Archived" && (
            <MenuItem onClick={() => run(() => onAction("maintenance"))} className="text-amber-600 dark:text-amber-400">
              Underhåll
            </MenuItem>
          )}
          {resource.status !== "Offline" && resource.status !== "Archived" && (
            <MenuItem onClick={() => run(() => onAction("offline"))} className="text-gray-500 dark:text-gray-400">
              Offline
            </MenuItem>
          )}
          {resource.status === "Archived" ? (
            <MenuItem onClick={() => run(() => onAction("unarchive"))} className="text-indigo-600 dark:text-indigo-400">
              Återställ
            </MenuItem>
          ) : (
            <MenuItem onClick={() => run(() => onAction("archive"))} className="text-red-600 dark:text-red-400">
              Arkivera
            </MenuItem>
          )}
        </div>
      )}
    </div>
  );
}

export function ResourcesAdminPage() {
  useResourceStatusUpdates();

  const [editing, setEditing] = useState<Resource | null>(null);
  const [creating, setCreating] = useState(false);
  const [bookingResource, setBookingResource] = useState<Resource | null>(null);
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
      {
        onError: (err) =>
          setError(err instanceof ApiError ? err.message : "Något gick fel"),
      },
    );
  }

  const resources = filterByResourceType(
    resourcesQuery.data ?? [],
    selectedTypeId,
  );

  return (
    <div>
      <div className="sticky top-14 z-10 -mx-4 px-4 bg-gray-50 dark:bg-gray-950 pt-4 pb-3 mb-1">
        <div className="flex flex-wrap items-center justify-between gap-3 mb-4">
          <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">
            Resurser (admin)
          </h1>
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
          className="mb-0"
        />
      </div>

      {error && <p className="text-sm text-red-600 mb-4">{error}</p>}
      {resourcesQuery.isLoading && <p className="text-gray-500">Laddar...</p>}

      <div className="hidden md:block overflow-x-auto rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900">
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
                  <div className="font-medium text-gray-900 dark:text-gray-100">
                    {resource.name}
                  </div>
                  <div className="text-gray-500 dark:text-gray-400">
                    {resource.description}
                  </div>
                </td>
                <td className="px-4 py-3 text-gray-600 dark:text-gray-300">
                  {typeNameById.get(resource.resourceTypeId) ?? "-"}
                </td>
                <td className="px-4 py-3">
                  <StatusBadge status={resource.status} />
                </td>
                <td className="px-4 py-3 text-right">
                  <ResourceActions
                    resource={resource}
                    onBook={() => setBookingResource(resource)}
                    onEdit={() => setEditing(resource)}
                    onAction={(action) => runAction(resource.id, action)}
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {resources.length === 0 && !resourcesQuery.isLoading && (
          <p className="text-sm text-gray-500 px-4 py-6">
            Inga resurser av den valda typen.
          </p>
        )}
      </div>

      <div className="md:hidden space-y-3">
        {resources.map((resource) => (
          <div
            key={resource.id}
            className="rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-4"
          >
            <div className="flex items-start justify-between gap-2">
              <div>
                <div className="font-medium text-gray-900 dark:text-gray-100">
                  {resource.name}
                </div>
                <div className="text-sm text-gray-500 dark:text-gray-400">
                  {resource.description}
                </div>
              </div>
              <StatusBadge status={resource.status} />
            </div>
            <div className="mt-1 text-sm text-gray-600 dark:text-gray-300">
              {typeNameById.get(resource.resourceTypeId) ?? "-"}
            </div>
            <div className="mt-3">
              <ResourceActions
                resource={resource}
                onBook={() => setBookingResource(resource)}
                onEdit={() => setEditing(resource)}
                onAction={(action) => runAction(resource.id, action)}
                align="left"
              />
            </div>
          </div>
        ))}
        {resources.length === 0 && !resourcesQuery.isLoading && (
          <p className="text-sm text-gray-500 px-1 py-6">
            Inga resurser av den valda typen.
          </p>
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

      {bookingResource && (
        <BookingModal resource={bookingResource} onClose={() => setBookingResource(null)} />
      )}
    </div>
  );
}
