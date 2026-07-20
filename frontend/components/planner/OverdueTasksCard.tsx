"use client";

import { AlertTriangle } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useRescheduleOverdueTasks } from "@/lib/hooks/usePlanner";
import type { DailyTask } from "@/lib/types/planner";

export function OverdueTasksCard({ tasks }: { tasks: DailyTask[] }) {
  const rescheduleOverdue = useRescheduleOverdueTasks();

  if (tasks.length === 0) return null;

  return (
    <Card className="border-amber-500/30 bg-amber-500/5">
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base text-amber-600 dark:text-amber-400">
          <AlertTriangle className="size-4" /> {tasks.length} overdue {tasks.length === 1 ? "task" : "tasks"}
        </CardTitle>
        <CardDescription>These were never completed on their original day.</CardDescription>
        <CardAction>
          <Button
            size="sm"
            variant="outline"
            onClick={() => rescheduleOverdue.mutate()}
            disabled={rescheduleOverdue.isPending}
          >
            {rescheduleOverdue.isPending ? "Rescheduling…" : "Reschedule all to today"}
          </Button>
        </CardAction>
      </CardHeader>
      <CardContent className="flex flex-col gap-2">
        {tasks.map((task) => (
          <div key={task.id} className="flex items-center justify-between gap-2 rounded-md border bg-background px-3 py-2 text-sm">
            <span className="truncate">{task.title}</span>
            <span className="shrink-0 text-xs text-muted-foreground">
              {new Date(`${task.date}T00:00:00`).toLocaleDateString(undefined, { month: "short", day: "numeric" })}
            </span>
          </div>
        ))}
      </CardContent>
    </Card>
  );
}
