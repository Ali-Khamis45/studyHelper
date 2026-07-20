export interface AiHealth {
  provider: string;
  model?: string;
  status: "healthy" | "offline";
  latencyMs?: number;
  message?: string;
}

export interface AgentMetrics {
  agentType: string;
  totalExecutions: number;
  successCount: number;
  failureCount: number;
  averageLatencyMs: number;
  retryRatePercent: number;
  averageTotalTokens: number;
  lastExecutionUtc: string;
}
