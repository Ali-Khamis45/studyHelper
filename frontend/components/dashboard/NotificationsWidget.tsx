"use client";

import { AlertTriangle, Bell, CalendarClock, Flame, TrendingDown } from "lucide-react";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { useTodayPlan } from "@/lib/hooks/usePlanner";
import { useDashboardAnalytics } from "@/lib/hooks/useAnalytics";

interface Alert {
  icon: typeof Bell;
  text: string;
  tone: "warning" | "info";
}

/// There's no persisted notification system (no dismiss/read state) — these are real alerts derived
/// on the fly from data every widget on this page already fetches (upcoming deadlines, weak topics,
/// today's completion, the AI's own risk detection), not a fabricated feed.
export function NotificationsWidget() {
  const { data: plan, isLoading: planLoading } = useTodayPlan();
  const { data: dashboard, isLoading: dashboardLoading } = useDashboardAnalytics();

  if (planLoading || dashboardLoading) return <Skeleton className="h-32 rounded-xl" />;

  const alerts: Alert[] = [];

  for (const deadline of plan?.upcomingDeadlines ?? []) {
    if (deadline.daysRemaining <= 3) {
      alerts.push({ icon: CalendarClock, text: `"${deadline.title}" is due in ${deadline.daysRemaining === 0 ? "today" : `${deadline.daysRemaining}d`}`, tone: "warning" });
    }
  }

  if (plan && plan.studyStreak > 0 && plan.dailyCompletionPercent === 0) {
    alerts.push({ icon: Flame, text: `Complete a task today to keep your ${plan.studyStreak}-day streak`, tone: "warning" });
  }

  for (const topic of (dashboard?.weakTopics ?? []).slice(0, 2)) {
    alerts.push({ icon: TrendingDown, text: `"${topic.topic}" mastery is at ${Math.round(topic.masteryScore * 100)}% — worth reviewing`, tone: "info" });
  }

  if (dashboard?.insights?.riskDetection) {
    alerts.push({ icon: AlertTriangle, text: dashboard.insights.riskDetection, tone: "warning" });
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base">
          <Bell className="size-4" />
          Notifications
        </CardTitle>
        <CardDescription>Derived from your real activity — not a saved feed.</CardDescription>
      </CardHeader>
      <CardContent>
        {alerts.length === 0 ? (
          <p className="text-sm text-muted-foreground">Nothing needs your attention right now.</p>
        ) : (
          <ul className="flex flex-col gap-2.5">
            {alerts.slice(0, 5).map((alert, i) => (
              <li key={i} className="flex items-start gap-2 text-sm">
                <alert.icon className={`mt-0.5 size-3.5 shrink-0 ${alert.tone === "warning" ? "text-amber-500" : "text-muted-foreground"}`} />
                {alert.text}
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  );
}
