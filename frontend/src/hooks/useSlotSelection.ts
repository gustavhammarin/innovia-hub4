import { useMemo, useState } from "react";
import type { AvailabilitySlot } from "../api/types";
import { durationMinutes } from "../lib/date";

interface Selection {
  dateKey: string;
  startIndex: number;
  endIndex: number;
}

export function useSlotSelection(
  slotsByDate: Map<string, AvailabilitySlot[]>,
  maxDurationMinutes?: number
) {
  const [selection, setSelection] = useState<Selection | null>(null);

  function toggleSlot(dateKey: string, index: number) {
    const slots = slotsByDate.get(dateKey) ?? [];

    if (!selection || selection.dateKey !== dateKey) {
      setSelection({ dateKey, startIndex: index, endIndex: index });
      return;
    }

    const newStart = Math.min(selection.startIndex, index);
    const newEnd = Math.max(selection.endIndex, index);
    const allAvailable = slots.slice(newStart, newEnd + 1).every((s) => s.isAvailable);

    setSelection(
      allAvailable
        ? { dateKey, startIndex: newStart, endIndex: newEnd }
        : { dateKey, startIndex: index, endIndex: index }
    );
  }

  function clear() {
    setSelection(null);
  }

  function isSelected(dateKey: string, index: number): boolean {
    return (
      selection?.dateKey === dateKey && index >= selection.startIndex && index <= selection.endIndex
    );
  }

  const selectedSlots = useMemo(() => {
    if (!selection) return [];
    const slots = slotsByDate.get(selection.dateKey);
    return slots ? slots.slice(selection.startIndex, selection.endIndex + 1) : [];
  }, [selection, slotsByDate]);

  const rangeStart = selectedSlots[0];
  const rangeEnd = selectedSlots[selectedSlots.length - 1];
  const duration = rangeStart && rangeEnd ? durationMinutes(rangeStart.startUtc, rangeEnd.endUtc) : 0;
  const exceedsMax = maxDurationMinutes != null && duration > maxDurationMinutes;

  return { toggleSlot, clear, isSelected, rangeStart, rangeEnd, duration, exceedsMax };
}
