import { useState } from "react";
import { addDaysIso, todayIso } from "../lib/date";

export function useWeekRange() {
  const [fromDate, setFromDate] = useState(todayIso());
  const toDate = addDaysIso(fromDate, 6);

  return {
    fromDate,
    toDate,
    nextWeek: () => setFromDate((d) => addDaysIso(d, 7)),
    previousWeek: () => setFromDate((d) => addDaysIso(d, -7)),
  };
}
