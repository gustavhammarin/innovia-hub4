import { useState } from "react";
import { ApiError } from "../../api/client";
import type { ResourceType } from "../../api/types";
import { useResourceTypes } from "../../hooks/useResources";
import { useSaveResourceTypeMutation } from "../../hooks/useResourceMutations";

interface FormState {
  id: string | null;
  name: string;
  maxDurationMinutes: number;
  maxAdvanceDays: number;
}

const emptyForm: FormState = { id: null, name: "", maxDurationMinutes: 60, maxAdvanceDays: 30 };

export function ResourceTypesAdminPage() {
  const [form, setForm] = useState<FormState>(emptyForm);
  const [error, setError] = useState<string | null>(null);

  const typesQuery = useResourceTypes();
  const mutation = useSaveResourceTypeMutation(form.id);

  function handleSubmit() {
    setError(null);
    mutation.mutate(
      {
        name: form.name,
        maxDurationMinutes: form.maxDurationMinutes,
        maxAdvanceDays: form.maxAdvanceDays,
      },
      {
        onSuccess: () => setForm(emptyForm),
        onError: (err) => setError(err instanceof ApiError ? err.message : "Något gick fel"),
      }
    );
  }

  function edit(type: ResourceType) {
    setForm({
      id: type.id,
      name: type.name,
      maxDurationMinutes: type.maxDurationMinutes,
      maxAdvanceDays: type.maxAdvanceDays,
    });
  }

  return (
    <div>
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100 mb-6">Resurstyper</h1>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 overflow-x-auto rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900">
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
                      onClick={() => edit(type)}
                      className="text-indigo-600 hover:text-indigo-500 font-medium"
                    >
                      Redigera
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <form
          onSubmit={(e) => {
            e.preventDefault();
            handleSubmit();
          }}
          className="rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-4 space-y-4 h-fit"
        >
          <h2 className="font-medium text-gray-900 dark:text-gray-100">
            {form.id ? "Redigera resurstyp" : "Ny resurstyp"}
          </h2>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Namn
            </label>
            <input
              required
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
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
              value={form.maxDurationMinutes}
              onChange={(e) => setForm({ ...form, maxDurationMinutes: Number(e.target.value) })}
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
              value={form.maxAdvanceDays}
              onChange={(e) => setForm({ ...form, maxAdvanceDays: Number(e.target.value) })}
              className="w-full rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>
          {error && <p className="text-sm text-red-600">{error}</p>}
          <div className="flex gap-2">
            <button
              type="submit"
              disabled={mutation.isPending}
              className="rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50"
            >
              {form.id ? "Spara" : "Skapa"}
            </button>
            {form.id && (
              <button
                type="button"
                onClick={() => setForm(emptyForm)}
                className="rounded-md px-3 py-1.5 text-sm font-medium text-gray-600 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-800"
              >
                Avbryt
              </button>
            )}
          </div>
        </form>
      </div>
    </div>
  );
}
