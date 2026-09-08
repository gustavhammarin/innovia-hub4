import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
  type HubConnection,
} from "@microsoft/signalr";

const BASE_URL = import.meta.env.VITE_API_URL ?? "https://localhost:7229";
const LeaveResourceGroup: string = "LeaveResourceGroup";
const JoinResourceGroup: string = "JoinResourceGroup";

let connection: HubConnection | null = null;
let startPromise: Promise<void> | null = null;
const joinedResourceIds = new Set<string>();

function buildConnection(): HubConnection {
  const conn = new HubConnectionBuilder()
    .withUrl(`${BASE_URL}/hubs/bookings`, { withCredentials: true })
    .configureLogging(LogLevel.Information)
    .withAutomaticReconnect()
    .build();

  conn.onreconnecting((err) => {
    console.warn("Booking hub connection lost, attempting to reconnect", err);
  });

  conn.onreconnected(async () => {
    for (const id of joinedResourceIds) {
      try {
        await conn.invoke(JoinResourceGroup, id);
      } catch (err) {
        console.error("Failed to rejoin resource group");
      }
    }
  });

  conn.onclose((err) => {
    console.error("Booking hub connection closed permanently", err);
    startPromise = null;
  });

  return conn;
}

export function getBookingHubConnection(): HubConnection {
    if (!connection){
        connection = buildConnection();
    }
    return connection;
}

export function ensureStarted(conn: HubConnection): Promise<void> {
    if (conn.state === HubConnectionState.Connected) return Promise.resolve();

    if (!startPromise) {
        startPromise = conn.start().catch((err) => {
            startPromise = null;
            throw err;
        })
    }
    return startPromise;
}

export async function joinResourceGroup(
    conn: HubConnection,
    resourceId: string,
) {
    await conn.invoke(JoinResourceGroup, resourceId);
    joinedResourceIds.add(resourceId);
}

export function leaveResourceGroup(conn: HubConnection, resourceId: string){
    joinedResourceIds.delete(resourceId);
    if (conn.state === HubConnectionState.Connected){
        conn.invoke(LeaveResourceGroup, resourceId);
    }
}
