"use client";

import Link from "next/link";
import { Sparkles } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { buttonVariants } from "@/components/ui/button";
import { TaskItem } from "@/components/planner/TaskItem";
import { PlannerStatsCards } from "@/components/planner/PlannerStatsCards";
import { useTodayPlan } from "@/lib/hooks/usePlanner";

export function TodayStatsWidget() {
  const { data: plan, isLoading } = useTodayPlan();

  if (isLoading) {
    return (
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Skeleton className="h-20 rounded-xl" />
        <Skeleton className="h-20 rounded-xl" />
        <Skeleton className="h-20 rounded-xl" />
      </div>
    );
  }

  return (
    <PlannerStatsCards
      studyStreak={plan?.studyStreak ?? 0}
      dailyFocusScore={plan?.dailyFocusScore ?? 0}
      dailyCompletionPercent={plan?.dailyCompletionPercent ?? 0}
    />
  );
}

export function TodayRecommendationWidget() {
  const { data: plan, isLoading } = useTodayPlan();

  if (isLoading) return <Skeleton className="h-40 rounded-xl" />;

  const recommendation = plan?.recommendation;

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base">
          <Sparkles className="size-4 text-primary" />
          Today&apos;s Recommendation
        </CardTitle>
        {recommendation && <CardDescription>{recommendation.recommendation}</CardDescription>}
      </CardHeader>
      <CardContent className="flex flex-col gap-2">
        {!recommendation ? (
          <p className="text-sm text-muted-foreground">No recommendation yet — visit the Planner to generate one.</p>
        ) : (
          <>
            <p className="text-sm">{recommendation.immediateNextAction}</p>
            <Badge variant="outline" className="w-fit">
              {Math.round(recommendation.confidenceScore * 100)}% confidence
            </Badge>
          </>
        )}
        <Link href="/planner" className={buttonVariants({ variant: "outline", size: "sm", className: "mt-2 w-fit" })}>
          Open Planner
        </Link>
      </CardContent>
    </Card>
  );
}

export function TodayTasksWidget() {
  const { data: plan, isLoading } = useTodayPlan();

  if (isLoading) {
    return (
      <div className="flex flex-col gap-2">
        <Skeleton className="h-16 rounded-lg" />
        <Skeleton className="h-16 rounded-lg" />
      </div>
    );
  }

  const tasks = plan?.tasks ?? [];

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Today&apos;s Tasks</CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-3">
        {tasks.length === 0 ? (
          <p className="text-sm text-muted-foreground">No tasks planned for today yet.</p>
        ) : (
          tasks.slice(0, 4).map((task) => <TaskItem key={task.id} task={task} />)
        )}
      </CardContent>
    </Card>
  );
}
