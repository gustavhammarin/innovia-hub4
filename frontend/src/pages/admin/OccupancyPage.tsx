import { useState } from "react";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  Cell,
} from "recharts";
import {
  useCurrentOccupancy,
  useRangeOccupancy,
} from "../../hooks/useOccupancy";

const COLORS = ["#3b82f6", "#8b5cf6", "#22c55e", "#ef4444", "#f59e0b"];

function CustomTooltip({ active, payload }: any) {
  if (!active || !payload || payload.length === 0) return null;

  const item = payload[0].payload;

  return (
    <div
      style={{
        backgroundColor: "#1f2937",
        border: "none",
        borderRadius: 8,
        padding: "8px 12px",
        color: "#f3f4f6",
      }}
    >
      <p style={{ margin: 0, fontWeight: 600 }}>{item.name}</p>
      <p style={{ margin: 0 }}>{item.percentage}% beläggning</p>
      <p style={{ margin: 0, fontSize: 12, color: "#9ca3af" }}>
        {item.detailText}
      </p>
    </div>
  );
}

export function OccupancyPage() {
  const [view, setView] = useState<"current" | "range">("current");
  const [from, setFrom] = useState(new Date().toISOString().split("T")[0]);
  const [to, setTo] = useState(new Date().toISOString().split("T")[0]);

  const currentQuery = useCurrentOccupancy();
  const rangeQuery = useRangeOccupancy(from, to);

  const data = view === "current" ? currentQuery.data : rangeQuery.data;
  const isLoading =
    view === "current" ? currentQuery.isLoading : rangeQuery.isLoading;

  const chartData =
    data?.byResourceType.map((rt) => ({
      name: rt.name,
      percentage: rt.percentage,
      detailText:
        "bookedCount" in rt
          ? `${rt.bookedCount} av ${rt.totalCount} bokade`
          : `${Math.round(rt.bookedHours)} av ${Math.round(rt.availableHours)} timmar`,
    })) ?? [];

  return (
    <div>
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-gray-100 mb-6">
        Beläggning
      </h1>

      <div className="flex gap-2 mb-4">
        <button
          onClick={() => setView("current")}
          className={`text-sm font-medium px-3 py-1 rounded-md ${view === "current" ? "bg-indigo-600 text-white" : "text-gray-500"}`}
        >
          Just nu
        </button>
        <button
          onClick={() => setView("range")}
          className={`text-sm font-medium px-3 py-1 rounded-md ${view === "range" ? "bg-indigo-600 text-white" : "text-gray-500"}`}
        >
          Datumspann
        </button>
      </div>

      {view === "range" && (
        <div className="flex gap-4 mb-4 items-end">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Från
            </label>
            <input
              type="date"
              value={from}
              onChange={(e) => setFrom(e.target.value)}
              className="rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-gray-100"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Till
            </label>
            <input
              type="date"
              value={to}
              onChange={(e) => setTo(e.target.value)}
              className="rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-gray-100"
            />
          </div>
        </div>
      )}

      {isLoading && <p className="text-gray-500">Laddar beläggning...</p>}

      {!isLoading && data && (
        <div className="rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-4">
          <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">
            Total beläggning:{" "}
            <span className="font-semibold text-gray-900 dark:text-gray-100">
              {data.totalPercentage}%
            </span>
          </p>

          <div style={{ width: "100%", height: 320 }}>
            <ResponsiveContainer>
              <BarChart data={chartData} margin={{ bottom: 24 }}>
                <XAxis dataKey="name" height={50} interval={0} />
                <YAxis domain={[0, 100]} tickFormatter={(v) => `${v}%`} />
                <Tooltip cursor={false} content={<CustomTooltip />} />
                <Bar
                  dataKey="percentage"
                  radius={[4, 4, 0, 0]}
                  background={{ fill: "#1f2937" }}
                >
                  {chartData.map((_, i) => (
                    <Cell key={i} fill={COLORS[i % COLORS.length]} />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>
      )}

      {!isLoading && view === "range" && (!from || !to) && (
        <p className="text-sm text-gray-400">
          Välj ett datumspann för att se beläggning.
        </p>
      )}
    </div>
  );
}
