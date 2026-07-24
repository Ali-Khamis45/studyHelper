import { API_BASE_URL, ApiError, apiFetch } from "@/lib/api/client";
import { useAuthStore } from "@/lib/stores/authStore";
import type { GenerateRoadmapInput, Roadmap, RoadmapGenerationStreamEvent, RoadmapSummary } from "@/lib/types/roadmap";

export function getRoadmaps() {
  return apiFetch<RoadmapSummary[]>("/api/v1/roadmaps");
}

export function getRoadmap(roadmapId: string) {
  return apiFetch<Roadmap>(`/api/v1/roadmaps/${roadmapId}`);
}

export function generateRoadmap(input: GenerateRoadmapInput) {
  return apiFetch<Roadmap>("/api/v1/roadmaps/generate", { method: "POST", body: input });
}

// NDJSON, not apiFetch — mirrors streamGenerateQuiz / streamRecommendation.
export async function streamGenerateRoadmap(input: GenerateRoadmapInput, onEvent: (event: RoadmapGenerationStreamEvent) => void, signal?: AbortSignal) {
  const accessToken = useAuthStore.getState().accessToken;

  const res = await fetch(`${API_BASE_URL}/api/v1/roadmaps/generate/stream`, {
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
      if (line) onEvent(JSON.parse(line) as RoadmapGenerationStreamEvent);
    }
  }
}

export function deleteRoadmap(roadmapId: string) {
  return apiFetch<void>(`/api/v1/roadmaps/${roadmapId}`, { method: "DELETE" });
}

export function completeTopic(roadmapId: string, topicId: string, completed: boolean) {
  return apiFetch<void>(`/api/v1/roadmaps/${roadmapId}/topics/${topicId}/complete`, { method: "PATCH", body: { completed } });
}
