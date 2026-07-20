export type DailyTaskStatus = "Pending" | "Completed" | "Skipped";
export type TaskSource = "AiGenerated" | "Manual";
export type EnergyLevel = "Low" | "Medium" | "High";

export interface DailyTask {
  id: string;
  goalId: string | null;
  goalTitle: string | null;
  title: string;
  reasoning: string | null;
  date: string;
  estimatedMinutes: number;
  status: DailyTaskStatus;
  source: TaskSource;
  energyLevel: EnergyLevel | null;
  isOverdue: boolean;
  completedAtUtc: string | null;
}

export interface PlannerRecommendation {
  id: string;
  date: string;
  situationAnalysis: string;
  goalAlignment: string;
  evidence: string;
  recommendation: string;
  immediateNextAction: string;
  modelUsed: string;
  provider: string;
  promptVersion: string | null;
  confidenceScore: number;
  recommendationReason: string;
  generationTimeMs: number;
  generatedAt: string;
  expiresAt: string;
}

export interface UpcomingDeadline {
  goalId: string;
  title: string;
  targetDate: string;
  daysRemaining: number;
}

export interface TodayPlan {
  recommendation: PlannerRecommendation | null;
  tasks: DailyTask[];
  upcomingDeadlines: UpcomingDeadline[];
  overdueTasks: DailyTask[];
  dailyCompletionPercent: number;
  dailyFocusScore: number;
  studyStreak: number;
}

export interface WeekDay {
  date: string;
  tasks: DailyTask[];
  totalEstimatedMinutes: number;
  isOverloaded: boolean;
}

export interface Week {
  days: WeekDay[];
  weeklyCompletionPercent: number;
}

export interface RescheduleOverdueTasksResult {
  rescheduledCount: number;
}

export type RecommendationStreamEvent =
  | { type: "delta"; content: string }
  | { type: "complete"; plan: TodayPlan }
  | { type: "error"; message: string };
