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
