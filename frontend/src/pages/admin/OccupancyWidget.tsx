import {
  PieChart,
  Pie,
  Sector,
  ResponsiveContainer,
  Legend,
  Tooltip,
  type PieSectorShapeProps,
} from "recharts";
import { useOccupancyView } from "../../hooks/useOccupancy";

const COLORS = ["#3b82f6", "#8b5cf6", "#22c55e", "#ef4444", "#f59e0b"];

type Props = {
  from: string;
  to: string;
};

export function OccupancyWidget({ from, to }: Props) {
  const { view, setView, data, isLoading } = useOccupancyView(from, to);

  const totalBooked =
    data?.byResourceType.reduce(
      (sum, rt) =>
        sum + ("bookedCount" in rt ? rt.bookedCount : rt.bookedHours),
      0,
    ) ?? 0;

  const chartData =
    data?.byResourceType.map((rt) => ({
      name: rt.name,
      value: "bookedCount" in rt ? rt.bookedCount : rt.bookedHours,
    })) ?? [];

  return (
    <div className="rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-4 mb-6">
      <div className="flex gap-2 mb-4">
        <button
          onClick={() => setView("current")}
          className={`text-sm font-medium px-3 py-1 rounded-md ${view === "current" ? "bg-indigo-600 text-white" : "text-gray-500"}`}
        >
          Just nu
        </button>
        <button
          onClick={() => setView("range")}
          disabled={!from || !to}
          className={`text-sm font-medium px-3 py-1 rounded-md disabled:opacity-40 ${view === "range" ? "bg-indigo-600 text-white" : "text-gray-500"}`}
        >
          Valt datumspann
        </button>
      </div>

      {isLoading && <p className="text-gray-500">Laddar beläggning...</p>}

      {!isLoading && data && (
        <div className="flex items-center gap-6 flex-wrap">
          <div>
            <p className="text-sm text-gray-500 dark:text-gray-400">
              {view === "current"
                ? "Beläggning just nu"
                : "Beläggning för perioden"}
            </p>
            <p className="text-3xl font-semibold text-gray-900 dark:text-gray-100">
              {data.totalPercentage}%
            </p>
          </div>
          {totalBooked === 0 ? (
            <p className="text-sm text-gray-400">Inget bokat</p>
          ) : (
            <div style={{ width: 320, height: 200, maxWidth: "100%" }}>
              <ResponsiveContainer>
                <PieChart>
                  <Pie
                    data={chartData}
                    dataKey="value"
                    nameKey="name"
                    innerRadius={40}
                    outerRadius={65}
                    shape={(props: PieSectorShapeProps) => (
                      <Sector {...props} fill={COLORS[props.index % COLORS.length]} />
                    )}
                  />
                  <Tooltip />
                  <Legend
                    layout="vertical"
                    align="right"
                    verticalAlign="middle"
                    wrapperStyle={{ fontSize: 12 }}
                  />
                </PieChart>
              </ResponsiveContainer>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
