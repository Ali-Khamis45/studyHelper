import { apiFetch } from "@/lib/api/client";
import type { CreateGoalInput, Goal, GoalStatus, UpdateGoalInput } from "@/lib/types/goals";

export function getGoals(status?: GoalStatus) {
  const query = status ? `?status=${status}` : "";
  return apiFetch<Goal[]>(`/api/v1/goals${query}`);
}

export function createGoal(input: CreateGoalInput) {
  return apiFetch<Goal>("/api/v1/goals", { method: "POST", body: input });
}

export function updateGoal(id: string, input: UpdateGoalInput) {
  return apiFetch<Goal>(`/api/v1/goals/${id}`, { method: "PUT", body: input });
}

export function deleteGoal(id: string) {
  return apiFetch<void>(`/api/v1/goals/${id}`, { method: "DELETE" });
}
