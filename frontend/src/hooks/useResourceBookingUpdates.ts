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