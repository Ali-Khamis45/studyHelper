"use client";

import { useQuery } from "@tanstack/react-query";

import { getAiHealth } from "@/lib/api/system";

export function useAiHealth() {
  return useQuery({
    queryKey: ["system", "ai-health"],
    queryFn: getAiHealth,
    refetchInterval: 30_000,
    retry: false,
  });
}
