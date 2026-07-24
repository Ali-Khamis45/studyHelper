import { RevealGroup, RevealItem } from "@/components/motion/Reveal";
import { WidgetErrorBoundary } from "@/components/dashboard/WidgetErrorBoundary";
import { WelcomeWidget } from "@/components/dashboard/WelcomeWidget";
import { TodayRecommendationWidget, TodayStatsWidget, TodayTasksWidget } from "@/components/dashboard/TodayWidget";
import { GoalProgressWidget } from "@/components/dashboard/GoalProgressWidget";
import { RecentConversationWidget } from "@/components/dashboard/RecentConversationWidget";
import { NotificationsWidget } from "@/components/dashboard/NotificationsWidget";
import { QuickActionsWidget } from "@/components/dashboard/QuickActionsWidget";
import { RoadmapWidget } from "@/components/dashboard/RoadmapWidget";
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
    <RevealGroup className="mx-auto flex max-w-6xl flex-col gap-6">
      <RevealItem>
        <WidgetErrorBoundary label="the welcome banner">
          <WelcomeWidget />
        </WidgetErrorBoundary>
      </RevealItem>

      <RevealItem>
        <WidgetErrorBoundary label="today's stats">
          <TodayStatsWidget />
        </WidgetErrorBoundary>
      </RevealItem>

      <div className="grid gap-4 lg:grid-cols-3">
        <div className="flex flex-col gap-4 lg:col-span-2">
          <RevealItem>
            <WidgetErrorBoundary label="today's recommendation">
              <TodayRecommendationWidget />
            </WidgetErrorBoundary>
          </RevealItem>

          <RevealItem>
            <WidgetErrorBoundary label="today's tasks">
              <TodayTasksWidget />
            </WidgetErrorBoundary>
          </RevealItem>

          <RevealItem>
            <WidgetErrorBoundary label="weekly activity">
              <WeeklyActivityWidget />
            </WidgetErrorBoundary>
          </RevealItem>

          <RevealItem className="grid gap-4 sm:grid-cols-2">
            <WidgetErrorBoundary label="goal progress">
              <GoalProgressWidget />
            </WidgetErrorBoundary>
            <WidgetErrorBoundary label="mastery chart">
              <MasteryChartWidget />
            </WidgetErrorBoundary>
          </RevealItem>
        </div>

        <div className="flex flex-col gap-4">
          <RevealItem>
            <WidgetErrorBoundary label="quick actions">
              <QuickActionsWidget />
            </WidgetErrorBoundary>
          </RevealItem>

          <RevealItem>
            <WidgetErrorBoundary label="learning journey">
              <RoadmapWidget />
            </WidgetErrorBoundary>
          </RevealItem>

          <RevealItem>
            <WidgetErrorBoundary label="analytics snapshot">
              <AnalyticsSnapshotWidget />
            </WidgetErrorBoundary>
          </RevealItem>

          <RevealItem>
            <WidgetErrorBoundary label="notifications">
              <NotificationsWidget />
            </WidgetErrorBoundary>
          </RevealItem>

          <RevealItem>
            <WidgetErrorBoundary label="upcoming deadlines">
              <UpcomingDeadlinesWidget />
            </WidgetErrorBoundary>
          </RevealItem>

          <RevealItem>
            <WidgetErrorBoundary label="recent Mentor conversation">
              <RecentConversationWidget />
            </WidgetErrorBoundary>
          </RevealItem>

          <RevealItem>
            <WidgetErrorBoundary label="weak quiz topics">
              <WeakTopicsWidget />
            </WidgetErrorBoundary>
          </RevealItem>

          <RevealItem>
            <WidgetErrorBoundary label="AI insights">
              <AiInsightsWidget />
            </WidgetErrorBoundary>
          </RevealItem>
        </div>
      </div>
    </RevealGroup>
  );
}
