import { apiFetch } from "@/lib/api/client";
import type { AgentMetrics, AiHealth } from "@/lib/types/system";

export function getAiHealth() {
  return apiFetch<AiHealth>("/api/v1/system/ai");
}

export function getAgentMetrics() {
  return apiFetch<AgentMetrics[]>("/api/v1/system/agents");
}
