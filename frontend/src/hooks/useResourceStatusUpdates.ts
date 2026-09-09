import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import {
  ensureStarted,
  getResourceHubConnection,
  joinResourceStatusGroup,
  leaveResourceStatusGroup,
} from "../lib/resourceHubConnection";

const ResourceEvents = {
  ResourceStatusChanged: "ResourceStatusChanged",
} as const;

export function useResourceStatusUpdates() {
  const queryClient = useQueryClient();

  useEffect(() => {
    const conn = getResourceHubConnection();

    function invalidate() {
      queryClient.invalidateQueries({ queryKey: ["resourcesAdmin"] });
      queryClient.invalidateQueries({ queryKey: ["resources"] });
      queryClient.invalidateQueries({ queryKey: ["occupancy"] });
    }

    conn.on(ResourceEvents.ResourceStatusChanged, invalidate);

    (async () => {
      await ensureStarted(conn);
      await joinResourceStatusGroup(conn);
    })();

    return () => {
      conn.off(ResourceEvents.ResourceStatusChanged, invalidate);
      leaveResourceStatusGroup(conn);
    };
  }, [queryClient]);
}
