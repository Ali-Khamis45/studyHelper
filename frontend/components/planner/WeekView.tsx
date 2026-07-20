import { AlertTriangle } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import type { Week } from "@/lib/types/planner";
import { cn } from "@/lib/utils";

function formatMinutes(minutes: number) {
  if (minutes === 0) return null;
  const hours = Math.floor(minutes / 60);
  const remainder = minutes % 60;
  if (hours === 0) return `${remainder}m`;
  return remainder === 0 ? `${hours}h` : `${hours}h ${remainder}m`;
}

export function WeekView({ week }: { week: Week }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">This week</CardTitle>
        <CardDescription>{week.weeklyCompletionPercent.toFixed(0)}% of this week&apos;s tasks completed so far</CardDescription>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-4 lg:grid-cols-7">
          {week.days.map((day) => {
            const workload = formatMinutes(day.totalEstimatedMinutes);
            return (
              <div
                key={day.date}
                className={cn(
                  "flex min-h-24 flex-col gap-1.5 rounded-lg border p-2",
                  day.isOverloaded && "border-amber-500/40 bg-amber-500/5",
                )}
              >
                <div className="flex items-center justify-between gap-1">
                  <p className="text-xs font-medium text-muted-foreground">
                    {new Date(`${day.date}T00:00:00`).toLocaleDateString(undefined, { weekday: "short", day: "numeric" })}
                  </p>
                  {day.isOverloaded && <AlertTriangle className="size-3 shrink-0 text-amber-500" />}
                </div>
                {workload && (
                  <Badge variant={day.isOverloaded ? "outline" : "secondary"} className={cn("w-fit", day.isOverloaded && "border-amber-500/40 text-amber-600 dark:text-amber-400")}>
                    {workload}
                  </Badge>
                )}
                {day.tasks.length === 0 ? (
                  <p className="text-xs text-muted-foreground/50">—</p>
                ) : (
                  day.tasks.map((task) => (
                    <p
                      key={task.id}
                      title={task.title}
                      className={cn("truncate text-xs", task.status !== "Pending" && "text-muted-foreground line-through")}
                    >
                      {task.title}
                    </p>
                  ))
                )}
              </div>
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
}
