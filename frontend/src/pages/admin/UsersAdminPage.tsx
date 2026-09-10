import { useState } from "react";
import type { AppUser, UserFormData } from "../../api/types";
import { ApiError } from "../../api/client";
import { useAdminUsers, useCreateUser, useDeleteUser, useUpdateUser } from "../../hooks/useUsers";

const emptyForm: UserFormData = { firstName: "", lastName: "", email: "", password: "" };

export function UsersAdminPage() {
  const [search, setSearch] = useState("");
  const [selected, setSelected] = useState<AppUser | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [form, setForm] = useState<UserFormData>(emptyForm);
  const [error, setError] = useState<string | null>(null);
  const users = useAdminUsers(search);
  const create = useCreateUser();
  const update = useUpdateUser();
  const remove = useDeleteUser();
  const editing = selected !== null;

  function openCreate() { setSelected(null); setForm(emptyForm); setError(null); setIsFormOpen(true); }
  function openEdit(user: AppUser) { const parts = user.fullName.split(" "); setSelected(user); setForm({ firstName: parts.shift() ?? "", lastName: parts.join(" "), email: user.email, newPassword: "" }); setError(null); setIsFormOpen(true); }
  function closeForm() { setSelected(null); setForm(emptyForm); setError(null); setIsFormOpen(false); }
  function showError(err: unknown) { setError(err instanceof ApiError ? err.message : "Något gick fel."); }
  function submit(e: React.FormEvent) { e.preventDefault(); setError(null); if (!form.firstName?.trim() || !form.lastName?.trim() || !form.email?.trim() || (!editing && !form.password)) { setError("Fyll i alla obligatoriska fält."); return; } if (editing) update.mutate({ id: selected.id, data: { firstName: form.firstName.trim(), lastName: form.lastName.trim(), email: form.email.trim(), newPassword: form.newPassword || null } }, { onSuccess: closeForm, onError: showError }); else create.mutate({ firstName: form.firstName.trim(), lastName: form.lastName.trim(), email: form.email.trim(), password: form.password! }, { onSuccess: closeForm, onError: showError }); }
  function deleteUser(user: AppUser) { if (window.confirm(`Ta bort ${user.fullName} permanent? Aktiva och framtida bokningar avbryts.`)) remove.mutate(user.id, { onError: showError }); }

  return <div>
    <div className="mb-6 flex items-center justify-between"><div><h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100">Användare</h1><p className="mt-1 text-sm text-gray-500">Hantera konton och bevara bokningshistorik.</p></div><button onClick={openCreate} className="rounded-md bg-indigo-600 px-3 py-2 text-sm font-medium text-white hover:bg-indigo-500">Lägg till användare</button></div>
    <input value={search} onChange={e => setSearch(e.target.value)} placeholder="Sök namn eller e-post" className="mb-4 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm dark:border-gray-700 dark:bg-gray-900 dark:text-gray-100" />
    {users.isLoading && <p className="text-sm text-gray-500">Laddar användare…</p>}{users.isError && <p className="rounded-md bg-red-50 p-3 text-sm text-red-700">Kunde inte läsa användare.</p>}
    {!users.isLoading && !users.isError && <div className="overflow-x-auto rounded-xl border border-gray-200 bg-white dark:border-gray-800 dark:bg-gray-900"><table className="min-w-full divide-y divide-gray-200 text-sm dark:divide-gray-800"><thead><tr className="text-left text-xs font-semibold uppercase text-gray-400"><th className="px-4 py-3">Namn</th><th className="px-4 py-3">E-post</th><th className="px-4 py-3 text-right">Åtgärder</th></tr></thead><tbody className="divide-y divide-gray-100 dark:divide-gray-800">{(users.data ?? []).map(user => <tr key={user.id}><td className="px-4 py-3 font-medium text-gray-900 dark:text-gray-100">{user.fullName}</td><td className="px-4 py-3 text-gray-600 dark:text-gray-300">{user.email}</td><td className="space-x-3 px-4 py-3 text-right"><button onClick={() => openEdit(user)} className="font-medium text-indigo-600">Redigera</button><button onClick={() => deleteUser(user)} disabled={remove.isPending} className="font-medium text-red-600 disabled:opacity-50">Ta bort</button></td></tr>)}</tbody></table>{users.data?.length === 0 && <p className="p-6 text-center text-sm text-gray-500">Inga användare hittades.</p>}</div>}
    {isFormOpen && <div className="fixed inset-0 z-10 flex items-center justify-center bg-black/40 p-4"><form onSubmit={submit} className="w-full max-w-md space-y-4 rounded-xl bg-white p-6 shadow-xl dark:bg-gray-900"><h2 className="text-lg font-semibold dark:text-gray-100">{editing ? "Redigera användare" : "Ny användare"}</h2>{(["firstName", "lastName", "email"] as const).map(field => <input key={field} required value={form[field] ?? ""} onChange={e => setForm({ ...form, [field]: e.target.value })} placeholder={{ firstName: "Förnamn", lastName: "Efternamn", email: "E-post" }[field]} className="w-full rounded-md border px-3 py-2 text-sm dark:border-gray-700 dark:bg-gray-800 dark:text-gray-100" />)}<input required={!editing} type="password" value={editing ? form.newPassword ?? "" : form.password ?? ""} onChange={e => setForm({ ...form, ...(editing ? { newPassword: e.target.value } : { password: e.target.value }) })} placeholder={editing ? "Nytt lösenord (valfritt)" : "Lösenord"} className="w-full rounded-md border px-3 py-2 text-sm dark:border-gray-700 dark:bg-gray-800 dark:text-gray-100" />{error && <p className="text-sm text-red-600">{error}</p>}<div className="flex justify-end gap-2"><button type="button" onClick={closeForm} className="rounded-md px-3 py-2 text-sm text-gray-600">Avbryt</button><button disabled={create.isPending || update.isPending} className="rounded-md bg-indigo-600 px-3 py-2 text-sm text-white disabled:opacity-50">{editing ? "Spara" : "Skapa"}</button></div></form></div>}
  </div>;
}
