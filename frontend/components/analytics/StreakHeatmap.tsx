"use client";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { cn } from "@/lib/utils";
import type { HeatmapCell } from "@/lib/types/analytics";

function intensity(value: number, max: number) {
  if (value === 0) return "bg-muted";
  const ratio = max === 0 ? 0 : value / max;
  if (ratio > 0.75) return "bg-primary";
  if (ratio > 0.5) return "bg-primary/70";
  if (ratio > 0.25) return "bg-primary/45";
  return "bg-primary/25";
}

export function StreakHeatmap({ cells }: { cells: HeatmapCell[] }) {
  const byDate = new Map(cells.map((c) => [c.date, c.value]));
  const max = Math.max(1, ...cells.map((c) => c.value));

  const days: { date: string; value: number }[] = [];
  const today = new Date();
  for (let i = 89; i >= 0; i--) {
    const d = new Date(today);
    d.setDate(d.getDate() - i);
    const key = d.toISOString().slice(0, 10);
    days.push({ date: key, value: byDate.get(key) ?? 0 });
  }

  // Pad to a full first week so columns align to weekdays.
  const firstWeekday = new Date(days[0].date).getDay();
  const padded = [...Array.from({ length: firstWeekday }, () => null), ...days] as ({ date: string; value: number } | null)[];
  const weeks: ({ date: string; value: number } | null)[][] = [];
  for (let i = 0; i < padded.length; i += 7) weeks.push(padded.slice(i, i + 7));

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Completion Activity</CardTitle>
        <CardDescription>Last 90 days</CardDescription>
      </CardHeader>
      <CardContent>
        <div className="flex gap-1 overflow-x-auto pb-2">
          {weeks.map((week, wi) => (
            <div key={wi} className="flex flex-col gap-1">
              {week.map((day, di) =>
                day ? (
                  <div key={day.date} title={`${day.date}: ${day.value} completed`} className={cn("size-3 rounded-sm", intensity(day.value, max))} />
                ) : (
                  <div key={di} className="size-3" />
                ),
              )}
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
