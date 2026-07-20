"use client";

import { useState } from "react";
import { Plus, Target } from "lucide-react";

import { GoalCard } from "@/components/goals/GoalCard";
import { GoalFormDialog } from "@/components/goals/GoalFormDialog";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useGoals } from "@/lib/hooks/useGoals";
import type { Goal } from "@/lib/types/goals";

export default function GoalsPage() {
  const { data: goals, isLoading, isError } = useGoals();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingGoal, setEditingGoal] = useState<Goal | undefined>(undefined);

  const openCreate = () => {
    setEditingGoal(undefined);
    setDialogOpen(true);
  };

  const openEdit = (goal: Goal) => {
    setEditingGoal(goal);
    setDialogOpen(true);
  };

  return (
    <div className="mx-auto flex max-w-5xl flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Goals</h1>
          <p className="text-muted-foreground">What the planner builds your daily plan around.</p>
        </div>
        <Button onClick={openCreate}>
          <Plus /> New goal
        </Button>
      </div>

      {isLoading && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-44 rounded-xl" />
          ))}
        </div>
      )}

      {isError && (
        <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
          Couldn&apos;t load your goals. Try refreshing.
        </p>
      )}

      {goals && goals.length === 0 && (
        <div className="flex flex-col items-center gap-3 rounded-xl border border-dashed py-16 text-center">
          <span className="flex size-12 items-center justify-center rounded-2xl bg-accent text-accent-foreground">
            <Target className="size-5" />
          </span>
          <div>
            <p className="font-medium">No goals yet</p>
            <p className="text-sm text-muted-foreground">Create one to get a daily plan from the mentor.</p>
          </div>
          <Button onClick={openCreate}>
            <Plus /> New goal
          </Button>
        </div>
      )}

      {goals && goals.length > 0 && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {goals.map((goal) => (
            <GoalCard key={goal.id} goal={goal} onEdit={() => openEdit(goal)} />
          ))}
        </div>
      )}

      <GoalFormDialog open={dialogOpen} onOpenChange={setDialogOpen} goal={editingGoal} />
    </div>
  );
}
