"use client";

import { useAiHealth } from "@/lib/hooks/useSystem";
import { cn } from "@/lib/utils";

export function AiStatusBadge() {
  const { data } = useAiHealth();

  if (!data) return null;

  const healthy = data.status === "healthy";

  return (
    <span
      className={cn(
        "inline-flex items-center gap-1.5 rounded-full border px-2.5 py-1 text-xs font-medium",
        healthy
          ? "border-emerald-500/30 bg-emerald-500/10 text-emerald-600 dark:text-emerald-400"
          : "border-amber-500/30 bg-amber-500/10 text-amber-600 dark:text-amber-400",
      )}
      title={healthy ? `${data.provider}/${data.model} responded in ${data.latencyMs}ms` : data.message}
    >
      <span className={cn("size-1.5 rounded-full", healthy ? "bg-emerald-500" : "bg-amber-500")} />
      {healthy ? `${data.provider} · ${data.latencyMs}ms` : `${data.provider} offline`}
    </span>
  );
}
