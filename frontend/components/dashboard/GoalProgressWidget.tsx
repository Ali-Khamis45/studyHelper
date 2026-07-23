"use client";

import Link from "next/link";
import { Target } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import { Skeleton } from "@/components/ui/skeleton";
import { useGoalAnalytics } from "@/lib/hooks/useAnalytics";

export function GoalProgressWidget() {
  const { data, isLoading } = useGoalAnalytics();

  if (isLoading) return <Skeleton className="h-48 rounded-xl" />;

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base">
          <Target className="size-4" />
          Goal Progress
        </CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-3">
        {!data || data.totalGoals === 0 ? (
          <p className="text-sm text-muted-foreground">
            No goals yet.{" "}
            <Link href="/goals" className="underline">
              Create one
            </Link>
            .
          </p>
        ) : (
          <>
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">
                {data.completedGoals} of {data.totalGoals} completed
              </span>
              <span className="font-medium">{data.completionPercent}%</span>
            </div>
            <Progress value={data.completionPercent} />
            <ul className="flex flex-col gap-2 pt-1">
              {data.goals.slice(0, 3).map((goal) => (
                <li key={goal.id} className="flex items-center justify-between gap-2 text-sm">
                  <span className="truncate">{goal.title}</span>
                  <span className="shrink-0 text-xs text-muted-foreground">{goal.progressPercent}%</span>
                </li>
              ))}
            </ul>
          </>
        )}
      </CardContent>
    </Card>
  );
}
