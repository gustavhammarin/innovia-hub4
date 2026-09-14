import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
  type HubConnection,
} from "@microsoft/signalr";

const BASE_URL = import.meta.env.VITE_API_URL ?? "https://localhost:7229";
const LeaveResourceGroup: string = "LeaveResourceGroup";
const JoinResourceGroup: string = "JoinResourceGroup";

const JoinAdminBookingsGroup: string = "JoinAdminBookingsGroup";
const LeaveAdminBookingsGroup: string = "LeaveAdminBookingsGroup";
let adminGroupRefCount = 0;

let connection: HubConnection | null = null;
let startPromise: Promise<void> | null = null;
const resourceGroupRefCounts = new Map<string, number>();

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
    for (const id of resourceGroupRefCounts.keys()) {
      try {
        await conn.invoke(JoinResourceGroup, id);
      } catch (err) {
        console.error("Failed to rejoin resource group");
      }
    }
    if (adminGroupRefCount > 0)
      try {
        await conn.invoke(JoinAdminBookingsGroup);
      } catch (err) {
        console.error("Failed to rejoin Admin resourse group");
      }
  });

  conn.onclose((err) => {
    console.error("Booking hub connection closed permanently", err);
    startPromise = null;
  });

  return conn;
}

export function getBookingHubConnection(): HubConnection {
  if (!connection) {
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
    });
  }
  return startPromise;
}

export async function joinResourceGroup(
  conn: HubConnection,
  resourceId: string,
) {
  const count = resourceGroupRefCounts.get(resourceId) ?? 0;
  resourceGroupRefCounts.set(resourceId, count + 1);
  if (count === 0) {
    await conn.invoke(JoinResourceGroup, resourceId);
  }
}

export function leaveResourceGroup(conn: HubConnection, resourceId: string) {
  const count = resourceGroupRefCounts.get(resourceId) ?? 0;
  if (count <= 1) {
    resourceGroupRefCounts.delete(resourceId);
    if (conn.state === HubConnectionState.Connected) {
      conn.invoke(LeaveResourceGroup, resourceId);
    }
  } else {
    resourceGroupRefCounts.set(resourceId, count - 1);
  }
}

export async function joinAdminBookingsGroup(conn: HubConnection) {
  const count = adminGroupRefCount;
  adminGroupRefCount += 1;
  if (count === 0) {
    await conn.invoke(JoinAdminBookingsGroup);
  }
}

export function leaveAdminBookingsGroup(conn: HubConnection) {
  if (adminGroupRefCount <= 1) {
    adminGroupRefCount = 0;
    if (conn.state === HubConnectionState.Connected) {
      conn.invoke(LeaveAdminBookingsGroup);
    }
  } else {
    adminGroupRefCount -= 1;
  }
}
