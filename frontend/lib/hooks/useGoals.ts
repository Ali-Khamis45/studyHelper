"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { createGoal, deleteGoal, getGoals, updateGoal } from "@/lib/api/goals";
import type { CreateGoalInput, GoalStatus, UpdateGoalInput } from "@/lib/types/goals";

export function useGoals(status?: GoalStatus) {
  return useQuery({ queryKey: ["goals", status ?? "all"], queryFn: () => getGoals(status) });
}

export function useCreateGoal() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: CreateGoalInput) => createGoal(input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["goals"] }),
  });
}

export function useUpdateGoal() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateGoalInput }) => updateGoal(id, input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["goals"] }),
  });
}

export function useDeleteGoal() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteGoal(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["goals"] });
      // A deleted goal can be a task's goalId, and the planner shows goal titles/deadlines.
      queryClient.invalidateQueries({ queryKey: ["planner"] });
    },
  });
}
