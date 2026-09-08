import { useEffect, useState } from "react";
import { addDaysIso, todayIso } from "../lib/date";

export function useDayRange(maxAdvanceDays?: number) {
  const minDate = todayIso();
  const maxDate = maxAdvanceDays != null ? addDaysIso(minDate, maxAdvanceDays) : undefined;

  function clamp(candidate: string): string {
    if (candidate < minDate) return minDate;
    if (maxDate && candidate > maxDate) return maxDate;
    return candidate;
  }

  const [date, setDateState] = useState(minDate);

  useEffect(() => {
    setDateState((d) => clamp(d));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [maxDate]);

  function setDate(next: string) {
    setDateState(clamp(next));
  }

  function shift(days: number) {
    setDateState((d) => clamp(addDaysIso(d, days)));
  }

  return {
    date,
    setDate,
    nextDay: () => shift(1),
    previousDay: () => shift(-1),
    isToday: date === minDate,
    isMaxDate: maxDate != null && date === maxDate,
    minDate,
    maxDate,
  };
}
