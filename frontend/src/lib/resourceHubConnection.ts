import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
  type HubConnection,
} from "@microsoft/signalr";

const BASE_URL = import.meta.env.VITE_API_URL ?? "https://localhost:7229";
const JoinResourceStatusGroup: string = "JoinResourceStatusGroup";
const LeaveResourceStatusGroup: string = "LeaveResourceStatusGroup";

let connection: HubConnection | null = null;
let startPromise: Promise<void> | null = null;
let statusGroupJoined = false;

function buildConnection(): HubConnection {
  const conn = new HubConnectionBuilder()
    .withUrl(`${BASE_URL}/hubs/resources`, { withCredentials: true })
    .configureLogging(LogLevel.Information)
    .withAutomaticReconnect()
    .build();

  conn.onreconnecting((err) => {
    console.warn("Resource hub connection lost, attempting to reconnect", err);
  });

  conn.onreconnected(async () => {
    if (statusGroupJoined) {
      try {
        await conn.invoke(JoinResourceStatusGroup);
      } catch (err) {
        console.error("Failed to rejoin resource status group");
      }
    }
  });

  conn.onclose((err) => {
    console.error("Resource hub connection closed permanently", err);
    startPromise = null;
  });

  return conn;
}

export function getResourceHubConnection(): HubConnection {
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

export async function joinResourceStatusGroup(conn: HubConnection) {
  await conn.invoke(JoinResourceStatusGroup);
  statusGroupJoined = true;
}

export function leaveResourceStatusGroup(conn: HubConnection) {
  statusGroupJoined = false;
  if (conn.state === HubConnectionState.Connected) {
    conn.invoke(LeaveResourceStatusGroup);
  }
}
