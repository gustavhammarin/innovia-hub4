import { useQuery } from "@tanstack/react-query";
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
