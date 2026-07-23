import { API_BASE_URL, ApiError, apiFetch } from "@/lib/api/client";
import { useAuthStore } from "@/lib/stores/authStore";
import type {
  GenerateQuizInput,
  PagedResult,
  QuizAttemptResult,
  QuizDetail,
  QuizGenerationStreamEvent,
  QuizHistoryItem,
  QuizSummary,
  SubmittedAnswer,
  TopicMastery,
} from "@/lib/types/quiz";

export function getQuizzes(params: { page?: number; pageSize?: number } = {}) {
  const query = new URLSearchParams();
  if (params.page) query.set("page", String(params.page));
  if (params.pageSize) query.set("pageSize", String(params.pageSize));

  const qs = query.toString();
  return apiFetch<PagedResult<QuizSummary>>(`/api/v1/quiz${qs ? `?${qs}` : ""}`);
}

export function getQuiz(quizId: string) {
  return apiFetch<QuizDetail>(`/api/v1/quiz/${quizId}`);
}

export function generateQuiz(input: GenerateQuizInput) {
  return apiFetch<QuizDetail>("/api/v1/quiz/generate", { method: "POST", body: input });
}

// NDJSON, not apiFetch — mirrors streamRecommendation / streamMessage.
export async function streamGenerateQuiz(input: GenerateQuizInput, onEvent: (event: QuizGenerationStreamEvent) => void, signal?: AbortSignal) {
  const accessToken = useAuthStore.getState().accessToken;

  const res = await fetch(`${API_BASE_URL}/api/v1/quiz/generate/stream`, {
    method: "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
      ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
    },
    body: JSON.stringify(input),
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
      if (line) onEvent(JSON.parse(line) as QuizGenerationStreamEvent);
    }
  }
}

export function submitQuiz(quizId: string, answers: SubmittedAnswer[]) {
  return apiFetch<QuizAttemptResult>("/api/v1/quiz/submit", { method: "POST", body: { quizId, answers } });
}

export function retryQuiz(quizId: string) {
  return apiFetch<QuizDetail>(`/api/v1/quiz/${quizId}/retry`, { method: "POST" });
}

export function deleteQuiz(quizId: string) {
  return apiFetch<void>(`/api/v1/quiz/${quizId}`, { method: "DELETE" });
}

export function getQuizHistory(params: { page?: number; pageSize?: number } = {}) {
  const query = new URLSearchParams();
  if (params.page) query.set("page", String(params.page));
  if (params.pageSize) query.set("pageSize", String(params.pageSize));

  const qs = query.toString();
  return apiFetch<PagedResult<QuizHistoryItem>>(`/api/v1/quiz/history${qs ? `?${qs}` : ""}`);
}

export function getAttempt(attemptId: string) {
  return apiFetch<QuizAttemptResult>(`/api/v1/quiz/attempts/${attemptId}`);
}

export function getTopicMastery() {
  return apiFetch<TopicMastery[]>("/api/v1/quiz/mastery");
}

export function getWeakTopics(take?: number) {
  return apiFetch<TopicMastery[]>(`/api/v1/quiz/weak-topics${take ? `?take=${take}` : ""}`);
}
