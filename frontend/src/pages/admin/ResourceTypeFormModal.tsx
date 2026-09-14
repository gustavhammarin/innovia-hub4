import { useState } from "react";
import { Modal } from "../../components/Modal";
import { ApiError } from "../../api/client";
import type { ResourceType } from "../../api/types";
import { useSaveResourceTypeMutation } from "../../hooks/useResourceMutations";

export function ResourceTypeFormModal({
  resourceType,
  onClose,
}: {
  resourceType: ResourceType | null;
  onClose: () => void;
}) {
  const [name, setName] = useState(resourceType?.name ?? "");
  const [maxDurationMinutes, setMaxDurationMinutes] = useState(
    resourceType?.maxDurationMinutes ?? 60
  );
  const [maxAdvanceDays, setMaxAdvanceDays] = useState(
    resourceType?.maxAdvanceDays ?? 30
  );
  const [error, setError] = useState<string | null>(null);

  const mutation = useSaveResourceTypeMutation(resourceType?.id ?? null);

  function handleSubmit() {
    setError(null);
    mutation.mutate(
      { name, maxDurationMinutes, maxAdvanceDays },
      {
        onSuccess: onClose,
        onError: (err) => setError(err instanceof ApiError ? err.message : "Något gick fel"),
      }
    );
  }

  return (
    <Modal title={resourceType ? "Redigera resurstyp" : "Ny resurstyp"} onClose={onClose}>
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
            className="w-full rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Max bokningslängd (minuter)
          </label>
          <input
            type="number"
            required
            min={1}
            value={maxDurationMinutes}
            onChange={(e) => setMaxDurationMinutes(Number(e.target.value))}
            className="w-full rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Max förhandsbokning (dagar)
          </label>
          <input
            type="number"
            required
            min={1}
            value={maxAdvanceDays}
            onChange={(e) => setMaxAdvanceDays(Number(e.target.value))}
            className="w-full rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
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
            {resourceType ? "Spara" : "Skapa"}
          </button>
        </div>
      </form>
    </Modal>
  );
}
