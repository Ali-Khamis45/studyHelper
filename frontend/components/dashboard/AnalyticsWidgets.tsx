"use client";

import { Skeleton } from "@/components/ui/skeleton";
import { UpcomingDeadlines } from "@/components/planner/UpcomingDeadlines";
import { MasteryChart } from "@/components/quiz/MasteryChart";
import { WeakTopicsCard } from "@/components/quiz/WeakTopicsCard";
import { ActivityAreaChart } from "@/components/analytics/ActivityAreaChart";
import { InsightsPanel } from "@/components/analytics/InsightsPanel";
import { StatCard } from "@/components/analytics/StatCard";
import { useTodayPlan } from "@/lib/hooks/usePlanner";
import { useTopicMastery } from "@/lib/hooks/useQuiz";
import { useDashboardAnalytics } from "@/lib/hooks/useAnalytics";

export function UpcomingDeadlinesWidget() {
  const { data: plan, isLoading } = useTodayPlan();
  if (isLoading) return <Skeleton className="h-40 rounded-xl" />;
  return <UpcomingDeadlines deadlines={plan?.upcomingDeadlines ?? []} />;
}

export function MasteryChartWidget() {
  const { data: mastery, isLoading } = useTopicMastery();
  if (isLoading) return <Skeleton className="h-48 rounded-xl" />;
  return <MasteryChart mastery={mastery ?? []} />;
}

export function WeakTopicsWidget() {
  const { data, isLoading } = useDashboardAnalytics();
  if (isLoading) return <Skeleton className="h-40 rounded-xl" />;
  return <WeakTopicsCard topics={data?.weakTopics ?? []} />;
}

export function WeeklyActivityWidget() {
  const { data, isLoading } = useDashboardAnalytics();
  if (isLoading) return <Skeleton className="h-56 rounded-xl" />;
  return <ActivityAreaChart title="Weekly Activity" description="Minutes of completed study time, last 7 days" data={data?.weeklyActivity ?? []} unit=" min" />;
}

export function AiInsightsWidget() {
  const { data, isLoading } = useDashboardAnalytics();
  if (isLoading) return <Skeleton className="h-64 rounded-xl" />;
  return <InsightsPanel insights={data?.insights ?? null} />;
}

export function AnalyticsSnapshotWidget() {
  const { data: dashboard, isLoading: dashboardLoading } = useDashboardAnalytics();
  const { data: plan, isLoading: planLoading } = useTodayPlan();

  if (dashboardLoading || planLoading) {
    return (
      <div className="grid grid-cols-2 gap-3">
        <Skeleton className="h-20 rounded-xl" />
        <Skeleton className="h-20 rounded-xl" />
      </div>
    );
  }

  return (
    <div className="grid grid-cols-2 gap-3">
      <StatCard label="Active Goals" value={dashboard?.goals.totalGoals ?? 0} />
      <StatCard label="Today's Focus" value={plan?.dailyFocusScore ?? 0} />
    </div>
  );
}
