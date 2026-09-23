import { useMemo, useState } from "react";
import type { AggregatedSlot } from "../lib/availability";
import { durationMinutes } from "../lib/date";

interface Selection {
  startIndex: number;
  endIndex: number;
}

function intersect(a: string[], b: string[]): string[] {
  const set = new Set(b);
  return a.filter((id) => set.has(id));
}

function rangeResourceIds(slots: AggregatedSlot[], startIndex: number, endIndex: number): string[] {
  let result: string[] | null = null;
  for (const slot of slots.slice(startIndex, endIndex + 1)) {
    result = result === null ? slot.availableResourceIds : intersect(result, slot.availableResourceIds);
  }
  return result ?? [];
}

// A multi-slot range is only bookable if at least one resource is free across every slot in it,
// not merely if each slot has some resource free (those could be different resources).
export function useAggregatedSlotSelection(slots: AggregatedSlot[], maxDurationMinutes?: number) {
  const [selection, setSelection] = useState<Selection | null>(null);

  function toggleSlot(index: number) {
    if (slots[index].availableResourceIds.length === 0) return;

    if (!selection) {
      setSelection({ startIndex: index, endIndex: index });
      return;
    }

    const { startIndex, endIndex } = selection;

    if (index >= startIndex && index <= endIndex) {
      // Clicking an already-selected slot deselects it.
      if (startIndex === endIndex) {
        setSelection(null);
      } else if (index === startIndex) {
        setSelection({ startIndex: startIndex + 1, endIndex });
      } else if (index === endIndex) {
        setSelection({ startIndex, endIndex: endIndex - 1 });
      } else {
        // Deselecting from the middle would break the contiguous range a booking needs,
        // so restart the selection at the clicked slot instead.
        setSelection({ startIndex: index, endIndex: index });
      }
      return;
    }

    const newStart = Math.min(startIndex, index);
    const newEnd = Math.max(endIndex, index);
    const stillBookable = rangeResourceIds(slots, newStart, newEnd).length > 0;

    setSelection(
      stillBookable ? { startIndex: newStart, endIndex: newEnd } : { startIndex: index, endIndex: index }
    );
  }

  function clear() {
    setSelection(null);
  }

  function isSelected(index: number): boolean {
    return !!selection && index >= selection.startIndex && index <= selection.endIndex;
  }

  // Largest contiguous block starting at the first available slot, kept bookable
  // (same resource free throughout) and within the resource type's max duration.
  const fullDayRange = useMemo<Selection | null>(() => {
    const startIndex = slots.findIndex((s) => s.availableResourceIds.length > 0);
    if (startIndex === -1) return null;

    let endIndex = startIndex;
    let intersection = slots[startIndex].availableResourceIds;

    for (let i = startIndex + 1; i < slots.length; i++) {
      const slot = slots[i];
      if (slot.availableResourceIds.length === 0) break;
      const nextIntersection = intersect(intersection, slot.availableResourceIds);
      if (nextIntersection.length === 0) break;
      const candidateDuration = durationMinutes(slots[startIndex].startUtc, slot.endUtc);
      if (maxDurationMinutes != null && candidateDuration > maxDurationMinutes) break;
      intersection = nextIntersection;
      endIndex = i;
    }

    return { startIndex, endIndex };
  }, [slots, maxDurationMinutes]);

  function selectFullDay() {
    setSelection(fullDayRange);
  }

  // "Full day" only counts if the free block covers every slot of the day —
  // a short free stretch that happens to start at slot 0 doesn't qualify.
  const isFullDayAvailable =
    !!fullDayRange && fullDayRange.startIndex === 0 && fullDayRange.endIndex === slots.length - 1;

  const isFullDaySelected =
    !!selection &&
    !!fullDayRange &&
    selection.startIndex === fullDayRange.startIndex &&
    selection.endIndex === fullDayRange.endIndex;

  const rangeStart = selection ? slots[selection.startIndex] : undefined;
  const rangeEnd = selection ? slots[selection.endIndex] : undefined;
  const duration = rangeStart && rangeEnd ? durationMinutes(rangeStart.startUtc, rangeEnd.endUtc) : 0;
  const exceedsMax = maxDurationMinutes != null && duration > maxDurationMinutes;

  const availableResourceIds = useMemo(
    () => (selection ? rangeResourceIds(slots, selection.startIndex, selection.endIndex) : []),
    [selection, slots]
  );

  return {
    toggleSlot,
    clear,
    isSelected,
    rangeStart,
    rangeEnd,
    duration,
    exceedsMax,
    availableResourceIds,
    selectFullDay,
    isFullDaySelected,
    hasFullDayOption: isFullDayAvailable,
  };
}
