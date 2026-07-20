"use client";

import { MoreHorizontal, Pencil, Trash2 } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Progress } from "@/components/ui/progress";
import { useDeleteGoal } from "@/lib/hooks/useGoals";
import type { Goal } from "@/lib/types/goals";

const PRIORITY_VARIANT: Record<Goal["priority"], "default" | "secondary" | "outline"> = {
  High: "default",
  Medium: "secondary",
  Low: "outline",
};

export function GoalCard({ goal, onEdit }: { goal: Goal; onEdit: () => void }) {
  const deleteGoal = useDeleteGoal();

  return (
    <Card>
      <CardHeader>
        <div className="flex items-start justify-between gap-2">
          <CardTitle className="text-base">{goal.title}</CardTitle>
          <DropdownMenu>
            <DropdownMenuTrigger render={<Button variant="ghost" size="icon-sm" aria-label="Goal actions" />}>
              <MoreHorizontal />
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={onEdit}>
                <Pencil /> Edit
              </DropdownMenuItem>
              <DropdownMenuItem variant="destructive" onClick={() => deleteGoal.mutate(goal.id)}>
                <Trash2 /> Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
        {goal.description && <p className="text-sm text-muted-foreground">{goal.description}</p>}
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        <div className="flex flex-wrap items-center gap-2">
          <Badge variant={PRIORITY_VARIANT[goal.priority]}>{goal.priority}</Badge>
          <Badge variant="outline">{goal.category}</Badge>
          {goal.targetDate && (
            <span className="text-xs text-muted-foreground">
              Due {new Date(goal.targetDate).toLocaleDateString(undefined, { month: "short", day: "numeric", year: "numeric" })}
            </span>
          )}
        </div>

        <div className="flex flex-col gap-1.5">
          <div className="flex items-center justify-between text-xs text-muted-foreground">
            <span>Progress</span>
            <span>
              {goal.completedTasks}/{goal.totalTasks} tasks &middot; {goal.progressPercent}%
            </span>
          </div>
          <Progress value={goal.progressPercent} />
        </div>
      </CardContent>
    </Card>
  );
}
