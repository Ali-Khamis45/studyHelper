import { API_BASE_URL, ApiError, apiFetch } from "@/lib/api/client";
import { useAuthStore } from "@/lib/stores/authStore";
import type {
  PlannerRecommendation,
  RecommendationStreamEvent,
  RescheduleOverdueTasksResult,
  TodayPlan,
  Week,
} from "@/lib/types/planner";

export function getToday() {
  return apiFetch<TodayPlan>("/api/v1/planner/today");
}

export function generateRecommendation() {
  return apiFetch<TodayPlan>("/api/v1/planner/recommendations/generate", { method: "POST" });
}

// NDJSON, not apiFetch: the response body is a stream of one-JSON-object-per-line events, not a
// single JSON payload. Uses fetch() + a manual reader (not EventSource) because EventSource can't
// send the Authorization header this API requires.
export async function streamRecommendation(onEvent: (event: RecommendationStreamEvent) => void, signal?: AbortSignal) {
  const accessToken = useAuthStore.getState().accessToken;

  const res = await fetch(`${API_BASE_URL}/api/v1/planner/recommendations/stream`, {
    method: "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
      ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
    },
    signal,
  });

  if (!res.ok || !res.body) {
    const errorBody = await res.json().catch(() => null);
    throw new ApiError(res.status, errorBody);
  }

  const reader = res.body.getReader();
  const decoder = new TextDecoder();
  let buffer = "";

  while (true) {
    const { done, value } = await reader.read();
    if (done) break;

    buffer += decoder.decode(value, { stream: true });

    let newlineIndex: number;
    while ((newlineIndex = buffer.indexOf("\n")) >= 0) {
      const line = buffer.slice(0, newlineIndex).trim();
      buffer = buffer.slice(newlineIndex + 1);
      if (line) onEvent(JSON.parse(line) as RecommendationStreamEvent);
    }
  }
}

export function getWeek() {
  return apiFetch<Week>("/api/v1/planner/week");
}

export function completeTask(taskId: string) {
  return apiFetch<void>(`/api/v1/planner/tasks/${taskId}/complete`, { method: "PATCH" });
}

export function skipTask(taskId: string) {
  return apiFetch<void>(`/api/v1/planner/tasks/${taskId}/skip`, { method: "PATCH" });
}

export function rescheduleTask(taskId: string, newDate: string) {
  return apiFetch<void>(`/api/v1/planner/tasks/${taskId}/reschedule`, { method: "PATCH", body: { newDate } });
}

export function rescheduleOverdueTasks() {
  return apiFetch<RescheduleOverdueTasksResult>("/api/v1/planner/tasks/reschedule-overdue", { method: "POST" });
}

export function getRecommendationHistory() {
  return apiFetch<PlannerRecommendation[]>("/api/v1/planner/recommendations/history");
}
