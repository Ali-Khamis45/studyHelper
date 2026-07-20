export type GoalCategory = "Academic" | "Career" | "Skill" | "Certification" | "Personal" | "Other";
export type GoalStatus = "Active" | "Paused" | "Completed" | "Archived";
export type GoalPriority = "Low" | "Medium" | "High";

export interface Goal {
  id: string;
  title: string;
  description: string | null;
  category: GoalCategory;
  status: GoalStatus;
  priority: GoalPriority;
  targetDate: string | null;
  progressPercent: number;
  totalTasks: number;
  completedTasks: number;
  createdAtUtc: string;
}

export interface CreateGoalInput {
  title: string;
  description?: string | null;
  category: GoalCategory;
  priority: GoalPriority;
  targetDate?: string | null;
}

export type UpdateGoalInput = CreateGoalInput;

export const GOAL_CATEGORIES: GoalCategory[] = ["Academic", "Career", "Skill", "Certification", "Personal", "Other"];
export const GOAL_PRIORITIES: GoalPriority[] = ["Low", "Medium", "High"];
