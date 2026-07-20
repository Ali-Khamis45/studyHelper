"use client";

import { useState } from "react";
import { CalendarClock, Check, Clock, SkipForward } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useCompleteTask, useRescheduleTask, useSkipTask } from "@/lib/hooks/usePlanner";
import type { DailyTask } from "@/lib/types/planner";
import { cn } from "@/lib/utils";

export function TaskItem({ task }: { task: DailyTask }) {
  const [isRescheduling, setIsRescheduling] = useState(false);
  const [newDate, setNewDate] = useState(task.date);

  const completeTask = useCompleteTask();
  const skipTask = useSkipTask();
  const rescheduleTask = useRescheduleTask();

  const isPending = task.status === "Pending";

  return (
    <div className="flex flex-col gap-3 rounded-lg border p-3">
      <div className="flex flex-col gap-1">
        <div className="flex flex-wrap items-center gap-2">
          <p className={cn("text-sm font-medium", !isPending && "text-muted-foreground line-through")}>{task.title}</p>
          {task.status === "Completed" && <Badge variant="secondary">Completed</Badge>}
          {task.status === "Skipped" && <Badge variant="outline">Skipped</Badge>}
        </div>
        {task.reasoning && <p className="text-xs text-muted-foreground">{task.reasoning}</p>}
        <div className="flex flex-wrap items-center gap-2 pt-1">
          <span className="flex items-center gap-1 text-xs text-muted-foreground">
            <Clock className="size-3" />
            {task.estimatedMinutes} min
          </span>
          {task.goalTitle && <Badge variant="outline">{task.goalTitle}</Badge>}
        </div>
      </div>

      {isPending && !isRescheduling && (
        <div className="flex items-center gap-2">
          <Button size="sm" onClick={() => completeTask.mutate(task.id)} disabled={completeTask.isPending}>
            <Check /> Complete
          </Button>
          <Button size="sm" variant="outline" onClick={() => skipTask.mutate(task.id)} disabled={skipTask.isPending}>
            <SkipForward /> Skip
          </Button>
          <Button size="sm" variant="ghost" onClick={() => setIsRescheduling(true)}>
            <CalendarClock /> Reschedule
          </Button>
        </div>
      )}

      {isPending && isRescheduling && (
        <div className="flex items-center gap-2">
          <Input
            type="date"
            value={newDate}
            onChange={(e) => setNewDate(e.target.value)}
            className="h-8 w-auto"
          />
          <Button
            size="sm"
            disabled={rescheduleTask.isPending}
            onClick={() =>
              rescheduleTask.mutate(
                { taskId: task.id, newDate },
                { onSuccess: () => setIsRescheduling(false) },
              )
            }
          >
            Confirm
          </Button>
          <Button size="sm" variant="ghost" onClick={() => setIsRescheduling(false)}>
            Cancel
          </Button>
        </div>
      )}
    </div>
  );
}
