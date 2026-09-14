import type { AvailabilitySlot } from "../api/types";

export function groupSlotsByDate(slots: AvailabilitySlot[]): Map<string, AvailabilitySlot[]> {
  const byDate = new Map<string, AvailabilitySlot[]>();
  for (const slot of slots) {
    const dateKey = slot.startUtc.slice(0, 10);
    const list = byDate.get(dateKey) ?? [];
    list.push(slot);
    byDate.set(dateKey, list);
  }
  return byDate;
}

export interface AggregatedSlot {
  startUtc: string;
  endUtc: string;
  availableResourceIds: string[];
}

// All resources of one resource type share the same slot grid (rules are per-type),
// so slots from different resources line up by startUtc.
export function aggregateSlotsAcrossResources(
  resourceIds: string[],
  byResourceId: Map<string, AvailabilitySlot[]>
): AggregatedSlot[] {
  const byTime = new Map<string, AggregatedSlot>();
  for (const resourceId of resourceIds) {
    for (const slot of byResourceId.get(resourceId) ?? []) {
      const entry = byTime.get(slot.startUtc) ?? {
        startUtc: slot.startUtc,
        endUtc: slot.endUtc,
        availableResourceIds: [],
      };
      if (slot.isAvailable) entry.availableResourceIds.push(resourceId);
      byTime.set(slot.startUtc, entry);
    }
  }
  return Array.from(byTime.values()).sort((a, b) => a.startUtc.localeCompare(b.startUtc));
}

export function datesWithAvailability(
  resourceIds: string[],
  byResourceId: Map<string, AvailabilitySlot[]>
): Set<string> {
  const dates = new Set<string>();
  for (const resourceId of resourceIds) {
    for (const slot of byResourceId.get(resourceId) ?? []) {
      if (slot.isAvailable) dates.add(slot.startUtc.slice(0, 10));
    }
  }
  return dates;
}

// The availability endpoint only ever returns slots starting after "now", so there is no
// such thing as "the slot covering this instant" to check. Summarize by whether each
// resource has at least one free slot left today instead.
export function todayAvailabilitySummary(
  resourceIds: string[],
  byResourceId: Map<string, AvailabilitySlot[]>
): { availableCount: number; totalCount: number } {
  let availableCount = 0;
  for (const resourceId of resourceIds) {
    const slots = byResourceId.get(resourceId) ?? [];
    if (slots.some((s) => s.isAvailable)) availableCount += 1;
  }
  return { availableCount, totalCount: resourceIds.length };
}
