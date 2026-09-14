import { useState } from "react";
import { Modal } from "../../components/Modal";
import { ApiError } from "../../api/client";
import type { Resource, ResourceType } from "../../api/types";
import { useSaveResourceMutation } from "../../hooks/useResourceMutations";

export function ResourceFormModal({
  resource,
  resourceTypes,
  onClose,
}: {
  resource: Resource | null;
  resourceTypes: ResourceType[];
  onClose: () => void;
}) {
  const [name, setName] = useState(resource?.name ?? "");
  const [description, setDescription] = useState(resource?.description ?? "");
  const [resourceTypeId, setResourceTypeId] = useState(
    resource?.resourceTypeId ?? resourceTypes[0]?.id ?? ""
  );
  const [error, setError] = useState<string | null>(null);

  const mutation = useSaveResourceMutation(resource?.id ?? null);

  function handleSubmit() {
    setError(null);
    mutation.mutate(
      { name, description, resourceTypeId },
      {
        onSuccess: onClose,
        onError: (err) => setError(err instanceof ApiError ? err.message : "Något gick fel"),
      }
    );
  }

  return (
    <Modal title={resource ? "Redigera resurs" : "Ny resurs"} onClose={onClose}>
      <form
        onSubmit={(e) => {
          e.preventDefault();
          handleSubmit();
        }}
        className="space-y-4"
      >
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Namn
          </label>
          <input
            required
            value={name}
            onChange={(e) => setName(e.target.value)}
            className="w-full rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-base sm:text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Beskrivning
          </label>
          <textarea
            required
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            rows={3}
            className="w-full rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-base sm:text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Resurstyp
          </label>
          <select
            value={resourceTypeId}
            onChange={(e) => setResourceTypeId(e.target.value)}
            className="w-full rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-base sm:text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          >
            {resourceTypes.map((t) => (
              <option key={t.id} value={t.id}>
                {t.name}
              </option>
            ))}
          </select>
        </div>
        {error && <p className="text-sm text-red-600">{error}</p>}
        <div className="flex justify-end gap-2 pt-2">
          <button
            type="button"
            onClick={onClose}
            className="rounded-md px-3 py-1.5 text-sm font-medium text-gray-600 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-800"
          >
            Avbryt
          </button>
          <button
            type="submit"
            disabled={mutation.isPending}
            className="rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50"
          >
            {resource ? "Spara" : "Skapa"}
          </button>
        </div>
      </form>
    </Modal>
  );
}
