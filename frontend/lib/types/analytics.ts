export interface ChartPoint {
  label: string;
  value: number;
}

export interface PieSlice {
  label: string;
  value: number;
}

export interface HeatmapCell {
  date: string;
  value: number;
}

export interface RadarAxis {
  axis: string;
  value: number;
}

export interface TimelineEvent {
  occurredAtUtc: string;
  type: string;
  label: string;
}

export interface DistributionBucket {
  bucket: string;
  count: number;
}

export interface StudyTimeStats {
  dailyMinutes: number;
  weeklyMinutes: number;
  monthlyMinutes: number;
}

export interface TaskStats {
  completed: number;
  skipped: number;
  rescheduled: number;
  pending: number;
  total: number;
  completionRatePercent: number;
}

export interface GoalProgressItem {
  id: string;
  title: string;
  status: string;
  progressPercent: number;
}

export interface GoalAnalytics {
  totalGoals: number;
  completedGoals: number;
  completionPercent: number;
  goals: GoalProgressItem[];
}

export interface StreakAnalytics {
  currentStreak: number;
  longestStreak: number;
  completionHeatmap: HeatmapCell[];
}

export interface PlannerAnalytics {
  recommendationCount: number;
  acceptanceRatePercent: number;
  averageConfidence: number;
  averageGenerationTimeMs: number;
  generationTrend: ChartPoint[];
}

export interface QuizAnalytics {
  attemptCount: number;
  averageScore: number;
  highestScore: number;
  lowestScore: number;
  scoreTrend: ChartPoint[];
  scoreDistribution: DistributionBucket[];
}

export interface TopicMastery {
  topic: string;
  masteryScore: number;
  attemptsCount: number;
  lastUpdatedUtc: string;
}

export interface MasteryAnalytics {
  weakTopics: TopicMastery[];
  strongTopics: TopicMastery[];
  radar: RadarAxis[];
  evolutionTrend: ChartPoint[];
}

export interface MentorAnalytics {
  conversationCount: number;
  messageCount: number;
  averageSessionLengthMinutes: number;
}

export interface ProviderStat {
  provider: string;
  requestCount: number;
  averageLatencyMs: number;
}

export interface AiAnalytics {
  totalRequests: number;
  successRatePercent: number;
  failureRatePercent: number;
  averageLatencyMs: number;
  totalPromptTokens: number;
  totalCompletionTokens: number;
  byProvider: ProviderStat[];
}

export interface PeriodAnalytics {
  studyTime: StudyTimeStats;
  tasks: TaskStats;
  dailyActivity: ChartPoint[];
}

export interface Insights {
  weeklySummary: string;
  monthlySummary: string;
  strengths: string[];
  weaknesses: string[];
  recommendedFocusAreas: string[];
  riskDetection: string;
  suggestedScheduleImprovements: string[];
  generatedAtUtc: string;
}

export interface AnalyticsOverview {
  from: string;
  to: string;
  studyTime: StudyTimeStats;
  tasks: TaskStats;
  goals: GoalAnalytics;
  streak: StreakAnalytics;
  quizzes: QuizAnalytics;
  mastery: MasteryAnalytics;
  mentor: MentorAnalytics;
  ai: AiAnalytics;
  planner: PlannerAnalytics;
  timeline: TimelineEvent[];
  taskStatusDistribution: PieSlice[];
  insights: Insights | null;
}

export interface DashboardAnalytics {
  weakTopics: TopicMastery[];
  weeklyActivity: ChartPoint[];
  goals: GoalAnalytics;
  insights: Insights | null;
}

export type AnalyticsRangePreset = "today" | "week" | "month" | "year" | "custom";
