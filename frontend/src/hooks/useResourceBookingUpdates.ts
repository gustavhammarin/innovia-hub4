import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { ensureStarted, getBookingHubConnection, joinResourceGroup, leaveResourceGroup } from "../lib/bookingHubConnection";

const BookingEvents = {
    BookingCreated: "BookingCreated",
    BookingUpdated: "BookingUpdated",
    BookingCancelled: "BookingCancelled"
} as const;


export function useResourceBookingUpdates(resourceId: string){
    const queryClient = useQueryClient();

    useEffect(() => {
        const conn = getBookingHubConnection();

        function invalidateIfMine(payload: {resourceId: string}){
            if (payload.resourceId !== resourceId) return;
            queryClient.invalidateQueries({queryKey: ["availability", resourceId]});
        }

        conn.on(BookingEvents.BookingCreated, invalidateIfMine);
        conn.on(BookingEvents.BookingUpdated, invalidateIfMine);
        conn.on(BookingEvents.BookingCancelled, invalidateIfMine);

        (async () => {
            await ensureStarted(conn);
            await joinResourceGroup(conn, resourceId);
        })();

        return () => {
            conn.off(BookingEvents.BookingCreated, invalidateIfMine);
            conn.off(BookingEvents.BookingUpdated, invalidateIfMine);
            conn.off(BookingEvents.BookingCancelled, invalidateIfMine);
            leaveResourceGroup(conn, resourceId);
        }
    }, [resourceId, queryClient])
}

export function useResourcesBookingUpdates(resourceIds: string[]) {
    const queryClient = useQueryClient();
    const key = resourceIds.join(",");

    useEffect(() => {
        if (resourceIds.length === 0) return;
        const conn = getBookingHubConnection();
        const idSet = new Set(resourceIds);

        function invalidateIfMine(payload: {resourceId: string}){
            if (!idSet.has(payload.resourceId)) return;
            queryClient.invalidateQueries({queryKey: ["availability", payload.resourceId]});
        }

        conn.on(BookingEvents.BookingCreated, invalidateIfMine);
        conn.on(BookingEvents.BookingUpdated, invalidateIfMine);
        conn.on(BookingEvents.BookingCancelled, invalidateIfMine);

        (async () => {
            await ensureStarted(conn);
            await Promise.all(resourceIds.map((id) => joinResourceGroup(conn, id)));
        })();

        return () => {
            conn.off(BookingEvents.BookingCreated, invalidateIfMine);
            conn.off(BookingEvents.BookingUpdated, invalidateIfMine);
            conn.off(BookingEvents.BookingCancelled, invalidateIfMine);
            resourceIds.forEach((id) => leaveResourceGroup(conn, id));
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [key, queryClient])
}