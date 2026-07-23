import { WidgetErrorBoundary } from "@/components/dashboard/WidgetErrorBoundary";
import { WelcomeWidget } from "@/components/dashboard/WelcomeWidget";
import { TodayRecommendationWidget, TodayStatsWidget, TodayTasksWidget } from "@/components/dashboard/TodayWidget";
import { GoalProgressWidget } from "@/components/dashboard/GoalProgressWidget";
import { RecentConversationWidget } from "@/components/dashboard/RecentConversationWidget";
import { NotificationsWidget } from "@/components/dashboard/NotificationsWidget";
import { QuickActionsWidget } from "@/components/dashboard/QuickActionsWidget";
import {
  AiInsightsWidget,
  AnalyticsSnapshotWidget,
  MasteryChartWidget,
  UpcomingDeadlinesWidget,
  WeakTopicsWidget,
  WeeklyActivityWidget,
} from "@/components/dashboard/AnalyticsWidgets";

export default function DashboardPage() {
  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-6">
      <WidgetErrorBoundary label="the welcome banner">
        <WelcomeWidget />
      </WidgetErrorBoundary>

      <WidgetErrorBoundary label="today's stats">
        <TodayStatsWidget />
      </WidgetErrorBoundary>

      <div className="grid gap-4 lg:grid-cols-3">
        <div className="flex flex-col gap-4 lg:col-span-2">
          <WidgetErrorBoundary label="today's recommendation">
            <TodayRecommendationWidget />
          </WidgetErrorBoundary>

          <WidgetErrorBoundary label="today's tasks">
            <TodayTasksWidget />
          </WidgetErrorBoundary>

          <WidgetErrorBoundary label="weekly activity">
            <WeeklyActivityWidget />
          </WidgetErrorBoundary>

          <div className="grid gap-4 sm:grid-cols-2">
            <WidgetErrorBoundary label="goal progress">
              <GoalProgressWidget />
            </WidgetErrorBoundary>
            <WidgetErrorBoundary label="mastery chart">
              <MasteryChartWidget />
            </WidgetErrorBoundary>
          </div>
        </div>

        <div className="flex flex-col gap-4">
          <WidgetErrorBoundary label="quick actions">
            <QuickActionsWidget />
          </WidgetErrorBoundary>

          <WidgetErrorBoundary label="analytics snapshot">
            <AnalyticsSnapshotWidget />
          </WidgetErrorBoundary>

          <WidgetErrorBoundary label="notifications">
            <NotificationsWidget />
          </WidgetErrorBoundary>

          <WidgetErrorBoundary label="upcoming deadlines">
            <UpcomingDeadlinesWidget />
          </WidgetErrorBoundary>

          <WidgetErrorBoundary label="recent Mentor conversation">
            <RecentConversationWidget />
          </WidgetErrorBoundary>

          <WidgetErrorBoundary label="weak quiz topics">
            <WeakTopicsWidget />
          </WidgetErrorBoundary>

          <WidgetErrorBoundary label="AI insights">
            <AiInsightsWidget />
          </WidgetErrorBoundary>
        </div>
      </div>
    </div>
  );
}
