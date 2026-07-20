import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { Week } from "@/lib/types/planner";
import { cn } from "@/lib/utils";

export function WeekView({ week }: { week: Week }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">This week</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-4 lg:grid-cols-7">
          {week.days.map((day) => (
            <div key={day.date} className="flex min-h-24 flex-col gap-1.5 rounded-lg border p-2">
              <p className="text-xs font-medium text-muted-foreground">
                {new Date(`${day.date}T00:00:00`).toLocaleDateString(undefined, { weekday: "short", day: "numeric" })}
              </p>
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
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
