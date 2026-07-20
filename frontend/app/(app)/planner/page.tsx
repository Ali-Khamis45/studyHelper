"use client";

import { AiStatusBadge } from "@/components/planner/AiStatusBadge";
import { DailyLoopCard } from "@/components/planner/DailyLoopCard";
import { GoalProgressList } from "@/components/planner/GoalProgressList";
import { OverdueTasksCard } from "@/components/planner/OverdueTasksCard";
import { PlannerStatsCards } from "@/components/planner/PlannerStatsCards";
import { RecommendationHistoryCard } from "@/components/planner/RecommendationHistoryCard";
import { TaskItem } from "@/components/planner/TaskItem";
import { UpcomingDeadlines } from "@/components/planner/UpcomingDeadlines";
import { WeekView } from "@/components/planner/WeekView";
import { Skeleton } from "@/components/ui/skeleton";
import { useGoals } from "@/lib/hooks/useGoals";
import { useTodayPlan, useWeekPlan } from "@/lib/hooks/usePlanner";

export default function PlannerPage() {
  const { data: plan, isLoading: planLoading, isError: planError } = useTodayPlan();
  const { data: week, isLoading: weekLoading } = useWeekPlan();
  const { data: activeGoals } = useGoals("Active");

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Study Planner</h1>
          <p className="text-muted-foreground">
            {new Date().toLocaleDateString(undefined, { weekday: "long", month: "long", day: "numeric" })}
          </p>
        </div>
        <AiStatusBadge />
      </div>

      {planLoading && (
        <div className="grid gap-6 lg:grid-cols-3">
          <div className="flex flex-col gap-4 lg:col-span-2">
            <Skeleton className="h-24 rounded-xl" />
            <Skeleton className="h-56 rounded-xl" />
            <Skeleton className="h-40 rounded-xl" />
          </div>
          <Skeleton className="h-64 rounded-xl" />
        </div>
      )}

      {planError && (
        <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
          Couldn&apos;t reach the mentor to build today&apos;s plan. Make sure the AI provider
          (Ollama) is running locally, then reload this page.
        </p>
      )}

      {plan && (
        <>
          <PlannerStatsCards
            studyStreak={plan.studyStreak}
            dailyFocusScore={plan.dailyFocusScore}
            dailyCompletionPercent={plan.dailyCompletionPercent}
          />

          <div className="grid gap-6 lg:grid-cols-3">
            <div className="flex flex-col gap-4 lg:col-span-2">
              {plan.overdueTasks.length > 0 && <OverdueTasksCard tasks={plan.overdueTasks} />}

              <DailyLoopCard recommendation={plan.recommendation} />

              <div>
                <h2 className="mb-3 text-sm font-medium text-muted-foreground">Today&apos;s tasks</h2>
                {plan.tasks.length === 0 ? (
                  <p className="text-sm text-muted-foreground">No tasks for today yet.</p>
                ) : (
                  <div className="flex flex-col gap-2">
                    {plan.tasks.map((task) => (
                      <TaskItem key={task.id} task={task} />
                    ))}
                  </div>
                )}
              </div>
            </div>

            <div className="flex flex-col gap-4">
              <UpcomingDeadlines deadlines={plan.upcomingDeadlines} />
              {activeGoals && <GoalProgressList goals={activeGoals} />}
              <RecommendationHistoryCard />
            </div>
          </div>
        </>
      )}

      {weekLoading ? <Skeleton className="h-48 rounded-xl" /> : week && <WeekView week={week} />}
    </div>
  );
}
