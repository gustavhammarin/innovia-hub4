export function dateToIso(d: Date): string {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${y}-${m}-${day}`;
}

export function shiftMonth(iso: string, delta: number): string {
  const [y, m] = iso.split("-").map(Number);
  return dateToIso(new Date(y, m - 1 + delta, 1));
}

export function buildMonthMatrix(iso: string): { iso: string; dayNum: number; inMonth: boolean }[] {
  const [y, m] = iso.split("-").map(Number);
  const month = m - 1;
  const startOffset = (new Date(y, month, 1).getDay() + 6) % 7;

  return Array.from({ length: 42 }, (_, i) => {
    const cellDate = new Date(y, month, i - startOffset + 1);
    return {
      iso: dateToIso(cellDate),
      dayNum: cellDate.getDate(),
      inMonth: cellDate.getMonth() === month,
    };
  });
}

// The availability endpoint caps ranges at 31 days, so a single calendar month always fits.
export function monthAvailabilityRange(viewYm: string, minDate: string, maxDate?: string) {
  const [y, m] = viewYm.split("-").map(Number);
  const first = dateToIso(new Date(y, m - 1, 1));
  const last = dateToIso(new Date(y, m, 0));
  const from = first < minDate ? minDate : first;
  const to = maxDate && last > maxDate ? maxDate : last;
  return { from, to };
}
