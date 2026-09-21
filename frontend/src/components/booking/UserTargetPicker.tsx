import { useState } from "react";
import { useUserSearch } from "../../hooks/useUsers";
import type { AppUser } from "../../api/types";

export function UserTargetPicker({
  targetUser,
  onChange,
}: {
  targetUser: AppUser | null;
  onChange: (user: AppUser | null) => void;
}) {
  const [search, setSearch] = useState("");
  const [open, setOpen] = useState(false);
  const query = useUserSearch(search, open);

  return (
    <div className="mb-4">
      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
        Boka åt användare
      </label>
      {targetUser ? (
        <div className="flex items-center justify-between rounded-md border border-indigo-200 dark:border-indigo-800 bg-indigo-50 dark:bg-indigo-900/30 px-3 py-2">
          <div>
            <p className="text-sm font-medium text-gray-900 dark:text-gray-100">
              {targetUser.fullName}
            </p>
            <p className="text-xs text-gray-500 dark:text-gray-400">{targetUser.email}</p>
          </div>
          <button
            onClick={() => onChange(null)}
            className="text-sm text-gray-500 hover:text-gray-900 dark:hover:text-gray-100"
          >
            Byt
          </button>
        </div>
      ) : (
        <div className="relative">
          <input
            type="text"
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setOpen(true);
            }}
            onFocus={() => setOpen(true)}
            placeholder="Sök på namn eller e-post..."
            className="w-full rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-base sm:text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
          {open && (
            <div className="absolute z-10 mt-1 w-full rounded-md border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-lg max-h-48 overflow-y-auto">
              {query.isLoading && <p className="px-3 py-2 text-sm text-gray-500">Söker...</p>}
              {!query.isLoading && query.data?.length === 0 && (
                <p className="px-3 py-2 text-sm text-gray-500">Ingen användare hittades.</p>
              )}
              {query.data?.map((u) => (
                <button
                  key={u.id}
                  onClick={() => {
                    onChange(u);
                    setOpen(false);
                    setSearch("");
                  }}
                  className="block w-full text-left px-3 py-2 text-sm hover:bg-gray-50 dark:hover:bg-gray-800"
                >
                  <span className="font-medium text-gray-900 dark:text-gray-100">{u.fullName}</span>{" "}
                  <span className="text-gray-500 dark:text-gray-400">{u.email}</span>
                </button>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
