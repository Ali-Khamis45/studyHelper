"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  exportCsv,
  exportPdf,
  getDashboardAnalytics,
  getGoalAnalytics,
  getMasteryAnalytics,
  getMentorAnalytics,
  getMonthly,
  getOverview,
  getPlannerAnalytics,
  getQuizAnalytics,
  getStreak,
  getWeekly,
  regenerateInsights,
  triggerBrowserDownload,
} from "@/lib/api/analytics";

export function useAnalyticsOverview(range?: { from?: string; to?: string }) {
  return useQuery({
    queryKey: ["analytics", "overview", range?.from ?? null, range?.to ?? null],
    queryFn: () => getOverview(range),
  });
}

export function useDashboardAnalytics() {
  return useQuery({ queryKey: ["analytics", "dashboard"], queryFn: getDashboardAnalytics });
}

export function useWeeklyAnalytics() {
  return useQuery({ queryKey: ["analytics", "weekly"], queryFn: getWeekly });
}

export function useMonthlyAnalytics() {
  return useQuery({ queryKey: ["analytics", "monthly"], queryFn: getMonthly });
}

export function useStreakAnalytics() {
  return useQuery({ queryKey: ["analytics", "streak"], queryFn: getStreak });
}

export function useGoalAnalytics() {
  return useQuery({ queryKey: ["analytics", "goals"], queryFn: getGoalAnalytics });
}

export function useQuizAnalytics() {
  return useQuery({ queryKey: ["analytics", "quizzes"], queryFn: getQuizAnalytics });
}

export function useMasteryAnalytics() {
  return useQuery({ queryKey: ["analytics", "mastery"], queryFn: getMasteryAnalytics });
}

export function usePlannerAnalytics() {
  return useQuery({ queryKey: ["analytics", "planner"], queryFn: getPlannerAnalytics });
}

export function useMentorAnalytics() {
  return useQuery({ queryKey: ["analytics", "mentor"], queryFn: getMentorAnalytics });
}

export function useRegenerateInsights() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: regenerateInsights,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["analytics", "overview"] });
      queryClient.invalidateQueries({ queryKey: ["analytics", "dashboard"] });
    },
  });
}

export function useExportAnalytics() {
  return useMutation({
    mutationFn: async ({ format, range }: { format: "pdf" | "csv"; range?: { from?: string; to?: string } }) => {
      const blob = format === "pdf" ? await exportPdf(range) : await exportCsv(range);
      triggerBrowserDownload(blob, `analytics-report.${format}`);
    },
  });
}
