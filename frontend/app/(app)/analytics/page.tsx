"use client";

import { useMemo, useState } from "react";

import { Skeleton } from "@/components/ui/skeleton";
import { RangeFilter } from "@/components/analytics/RangeFilter";
import { StatCard } from "@/components/analytics/StatCard";
import { ActivityAreaChart } from "@/components/analytics/ActivityAreaChart";
import { TaskStatusPie } from "@/components/analytics/TaskStatusPie";
import { StreakHeatmap } from "@/components/analytics/StreakHeatmap";
import { MasteryRadar } from "@/components/analytics/MasteryRadar";
import { QuizScoreDistributionChart, QuizScoreTrendChart } from "@/components/analytics/QuizCharts";
import { TimelineFeed } from "@/components/analytics/TimelineFeed";
import { InsightsPanel } from "@/components/analytics/InsightsPanel";
import { ExportButtons } from "@/components/analytics/ExportButtons";
import { useAnalyticsOverview, useMonthlyAnalytics } from "@/lib/hooks/useAnalytics";
import type { AnalyticsRangePreset } from "@/lib/types/analytics";

function toIsoDate(date: Date) {
  return date.toISOString().slice(0, 10);
}

function rangeForPreset(preset: AnalyticsRangePreset, customFrom: string, customTo: string): { from?: string; to?: string } {
  const today = new Date();
  const to = toIsoDate(today);

  switch (preset) {
    case "today":
      return { from: to, to };
    case "week": {
      const from = new Date(today);
      from.setDate(from.getDate() - 6);
      return { from: toIsoDate(from), to };
    }
    case "month": {
      const from = new Date(today);
      from.setDate(from.getDate() - 29);
      return { from: toIsoDate(from), to };
    }
    case "year": {
      const from = new Date(today);
      from.setDate(from.getDate() - 364);
      return { from: toIsoDate(from), to };
    }
    case "custom":
      return { from: customFrom || undefined, to: customTo || undefined };
  }
}

export default function AnalyticsPage() {
  const [preset, setPreset] = useState<AnalyticsRangePreset>("month");
  const [customFrom, setCustomFrom] = useState("");
  const [customTo, setCustomTo] = useState("");

  const range = useMemo(() => rangeForPreset(preset, customFrom, customTo), [preset, customFrom, customTo]);
  const { data: overview, isLoading, isError } = useAnalyticsOverview(range);
  // Overview's own fields are current-state snapshots (see GetAnalyticsOverviewQueryHandler) — the
  // per-day activity series independent of the top-level range filter comes from its own endpoint.
  const { data: monthly } = useMonthlyAnalytics();

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-semibold tracking-tight">Analytics</h1>
          <p className="text-muted-foreground">A real, DB-computed picture of your study habits.</p>
        </div>
        <ExportButtons range={range} />
      </div>

      <RangeFilter preset={preset} onPresetChange={setPreset} customFrom={customFrom} customTo={customTo} onCustomChange={(f, t) => { setCustomFrom(f); setCustomTo(t); }} />

      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <Skeleton key={i} className="h-24 rounded-xl" />
          ))}
        </div>
      ) : isError || !overview ? (
        <p className="rounded-xl border border-dashed py-12 text-center text-sm text-muted-foreground">Couldn&apos;t load analytics. Please try again.</p>
      ) : (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <StatCard label="Study Time (today)" value={overview.studyTime.dailyMinutes} suffix="min" />
            <StatCard label="Task Completion" value={overview.tasks.completionRatePercent} suffix="%" />
            <StatCard label="Current Streak" value={overview.streak.currentStreak} suffix="days" />
            <StatCard label="Longest Streak" value={overview.streak.longestStreak} suffix="days" />
            <StatCard label="Goal Completion" value={overview.goals.completionPercent} suffix="%" />
            <StatCard label="Avg Quiz Score" value={overview.quizzes.averageScore} suffix="%" />
            <StatCard label="AI Success Rate" value={overview.ai.successRatePercent} suffix="%" />
            <StatCard label="Recommendation Acceptance" value={overview.planner.acceptanceRatePercent} suffix="%" />
          </div>

          <div className="grid gap-4 lg:grid-cols-3">
            <div className="flex flex-col gap-4 lg:col-span-2">
              <ActivityAreaChart title="Daily Activity" description="Minutes of completed study time per day — last 30 days" data={monthly?.dailyActivity ?? []} unit=" min" />
              <div className="grid gap-4 sm:grid-cols-2">
                <QuizScoreTrendChart data={overview.quizzes.scoreTrend} />
                <QuizScoreDistributionChart data={overview.quizzes.scoreDistribution} />
              </div>
              <StreakHeatmap cells={overview.streak.completionHeatmap} />
              <TimelineFeed events={overview.timeline} />
            </div>

            <div className="flex flex-col gap-4">
              <InsightsPanel insights={overview.insights} />
              <TaskStatusPie data={overview.taskStatusDistribution} />
              <MasteryRadar data={overview.mastery.radar} />
            </div>
          </div>
        </>
      )}
    </div>
  );
}
