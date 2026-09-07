export type ResourceStatus = "Online" | "Maintenance" | "Offline" | "Archived";

export interface Me {
  id: string;
  email: string;
  roles: string[];
}

export interface UserRef {
  userId: string;
  email: string;
}

export interface ResourceRef {
  resourceId: string;
  name: string;
  description: string;
}

export interface Resource {
  id: string;
  name: string;
  description: string;
  resourceTypeId: string;
  createdAt: string;
  status: ResourceStatus;
}

export interface ResourceType {
  id: string;
  name: string;
  createdAt: string;
  maxDurationMinutes: number;
  maxAdvanceDays: number;
}

export interface Booking {
  id: string;
  user: UserRef;
  resource: ResourceRef;
  startsAt: string;
  endsAt: string;
  createdAt: string;
  cancelledAt?: string | null;
}

export interface AvailabilitySlot {
  startUtc: string;
  endUtc: string;
  isAvailable: boolean;
}

export interface AvailabilityResponse {
  resourceId: string;
  slots: AvailabilitySlot[];
}
