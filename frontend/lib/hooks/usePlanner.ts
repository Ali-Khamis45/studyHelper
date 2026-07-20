"use client";

import { useCallback, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  completeTask,
  generateRecommendation,
  getRecommendationHistory,
  getToday,
  getWeek,
  rescheduleOverdueTasks,
  rescheduleTask,
  skipTask,
  streamRecommendation,
} from "@/lib/api/planner";

export function useTodayPlan() {
  return useQuery({ queryKey: ["planner", "today"], queryFn: getToday });
}

export function useWeekPlan() {
  return useQuery({ queryKey: ["planner", "week"], queryFn: getWeek });
}

function useInvalidatePlannerAndGoals() {
  const queryClient = useQueryClient();
  return () => {
    queryClient.invalidateQueries({ queryKey: ["planner"] });
    // Completing/skipping/rescheduling a task changes its goal's progress bar.
    queryClient.invalidateQueries({ queryKey: ["goals"] });
  };
}

export function useGenerateRecommendation() {
  const invalidate = useInvalidatePlannerAndGoals();
  return useMutation({ mutationFn: generateRecommendation, onSuccess: invalidate });
}

/// Streams the recommendation generation instead of waiting for the whole response (§4) — shows
/// incremental text as it arrives, then swaps the today-plan cache for the finalized plan once the
/// AI Kernel -> Provider -> API pipeline finishes and persists it via the same path useGenerateRecommendation uses.
export function useStreamRecommendation() {
  const queryClient = useQueryClient();
  const invalidateGoals = useInvalidatePlannerAndGoals();
  const [isStreaming, setIsStreaming] = useState(false);
  const [partialText, setPartialText] = useState("");
  const [error, setError] = useState<string | null>(null);

  const start = useCallback(async () => {
    setIsStreaming(true);
    setPartialText("");
    setError(null);

    try {
      await streamRecommendation((event) => {
        if (event.type === "delta") {
          setPartialText((prev) => prev + event.content);
        } else if (event.type === "complete") {
          queryClient.setQueryData(["planner", "today"], event.plan);
          queryClient.invalidateQueries({ queryKey: ["planner", "week"] });
          invalidateGoals();
        } else if (event.type === "error") {
          setError(event.message);
        }
      });
    } catch {
      setError("Couldn't generate a plan. Make sure Ollama is running locally, then try again.");
    } finally {
      setIsStreaming(false);
    }
  }, [queryClient, invalidateGoals]);

  return { start, isStreaming, partialText, error };
}

export function useCompleteTask() {
  const invalidate = useInvalidatePlannerAndGoals();
  return useMutation({ mutationFn: (taskId: string) => completeTask(taskId), onSuccess: invalidate });
}

export function useSkipTask() {
  const invalidate = useInvalidatePlannerAndGoals();
  return useMutation({ mutationFn: (taskId: string) => skipTask(taskId), onSuccess: invalidate });
}

export function useRescheduleTask() {
  const invalidate = useInvalidatePlannerAndGoals();
  return useMutation({
    mutationFn: ({ taskId, newDate }: { taskId: string; newDate: string }) => rescheduleTask(taskId, newDate),
    onSuccess: invalidate,
  });
}

export function useRescheduleOverdueTasks() {
  const invalidate = useInvalidatePlannerAndGoals();
  return useMutation({ mutationFn: rescheduleOverdueTasks, onSuccess: invalidate });
}

export function useRecommendationHistory() {
  return useQuery({ queryKey: ["planner", "history"], queryFn: getRecommendationHistory });
}
