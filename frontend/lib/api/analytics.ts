import { API_BASE_URL, apiFetch } from "@/lib/api/client";
import { useAuthStore } from "@/lib/stores/authStore";
import type {
  AnalyticsOverview,
  DashboardAnalytics,
  GoalAnalytics,
  Insights,
  MasteryAnalytics,
  MentorAnalytics,
  PeriodAnalytics,
  PlannerAnalytics,
  QuizAnalytics,
  StreakAnalytics,
} from "@/lib/types/analytics";

export function getOverview(range?: { from?: string; to?: string }) {
  const query = new URLSearchParams();
  if (range?.from) query.set("from", range.from);
  if (range?.to) query.set("to", range.to);
  const qs = query.toString();
  return apiFetch<AnalyticsOverview>(`/api/v1/analytics${qs ? `?${qs}` : ""}`);
}

export function getDashboardAnalytics() {
  return apiFetch<DashboardAnalytics>("/api/v1/analytics/dashboard");
}

export function getWeekly() {
  return apiFetch<PeriodAnalytics>("/api/v1/analytics/weekly");
}

export function getMonthly() {
  return apiFetch<PeriodAnalytics>("/api/v1/analytics/monthly");
}

export function getStreak() {
  return apiFetch<StreakAnalytics>("/api/v1/analytics/streak");
}

export function getGoalAnalytics() {
  return apiFetch<GoalAnalytics>("/api/v1/analytics/goals");
}

export function getQuizAnalytics() {
  return apiFetch<QuizAnalytics>("/api/v1/analytics/quizzes");
}

export function getMasteryAnalytics() {
  return apiFetch<MasteryAnalytics>("/api/v1/analytics/mastery");
}

export function getPlannerAnalytics() {
  return apiFetch<PlannerAnalytics>("/api/v1/analytics/planner");
}

export function getMentorAnalytics() {
  return apiFetch<MentorAnalytics>("/api/v1/analytics/mentor");
}

export function regenerateInsights() {
  return apiFetch<Insights>("/api/v1/analytics/insights/regenerate", { method: "POST" });
}

// File downloads bypass apiFetch (which always expects JSON) — fetch directly and hand back a Blob.
async function downloadFile(path: string, range?: { from?: string; to?: string }) {
  const query = new URLSearchParams();
  if (range?.from) query.set("from", range.from);
  if (range?.to) query.set("to", range.to);
  const qs = query.toString();

  const accessToken = useAuthStore.getState().accessToken;
  const res = await fetch(`${API_BASE_URL}${path}${qs ? `?${qs}` : ""}`, {
    credentials: "include",
    headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
  });

  if (!res.ok) throw new Error(`Export failed with status ${res.status}`);
  return res.blob();
}

export function exportPdf(range?: { from?: string; to?: string }) {
  return downloadFile("/api/v1/analytics/export/pdf", range);
}

export function exportCsv(range?: { from?: string; to?: string }) {
  return downloadFile("/api/v1/analytics/export/csv", range);
}

export function triggerBrowserDownload(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}
