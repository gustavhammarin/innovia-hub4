import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { usersApi } from "../api/users";
import { useDebouncedValue } from "./useDebouncedValue";

export function useUserSearch(search: string, enabled: boolean) {
  const debouncedSearch = useDebouncedValue(search, 300);

  return useQuery({
    queryKey: ["userSearch", debouncedSearch],
    queryFn: () => usersApi.list(debouncedSearch || undefined),
    enabled,
  });
}

export function useAdminUsers(search: string) {
  const debouncedSearch = useDebouncedValue(search, 300);
  return useQuery({ queryKey: ["adminUsers", debouncedSearch], queryFn: () => usersApi.list(debouncedSearch || undefined) });
}

export function useCreateUser() { const client = useQueryClient(); return useMutation({ mutationFn: usersApi.create, onSuccess: () => client.invalidateQueries({ queryKey: ["adminUsers"] }) }); }
export function useUpdateUser() { const client = useQueryClient(); return useMutation({ mutationFn: ({ id, data }: { id: string; data: Parameters<typeof usersApi.update>[1] }) => usersApi.update(id, data), onSuccess: () => client.invalidateQueries({ queryKey: ["adminUsers"] }) }); }
export function useDeleteUser() { const client = useQueryClient(); return useMutation({ mutationFn: usersApi.remove, onSuccess: () => client.invalidateQueries({ queryKey: ["adminUsers"] }) }); }
