import { z } from "zod";

import { GOAL_CATEGORIES, GOAL_PRIORITIES } from "@/lib/types/goals";

export const goalSchema = z.object({
  title: z.string().min(1, "Title is required").max(200),
  description: z.string().max(2000).optional(),
  category: z.enum(GOAL_CATEGORIES as [string, ...string[]]),
  priority: z.enum(GOAL_PRIORITIES as [string, ...string[]]),
  targetDate: z.string().optional(),
});

export type GoalFormValues = z.infer<typeof goalSchema>;
