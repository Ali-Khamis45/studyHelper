"use client";

import { useEffect } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";

import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { useCreateGoal, useUpdateGoal } from "@/lib/hooks/useGoals";
import { GOAL_CATEGORIES, GOAL_PRIORITIES, type Goal } from "@/lib/types/goals";
import { goalSchema, type GoalFormValues } from "@/lib/validation/goals";

export function GoalFormDialog({
  open,
  onOpenChange,
  goal,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  goal?: Goal;
}) {
  const isEditing = !!goal;
  const createGoal = useCreateGoal();
  const updateGoal = useUpdateGoal();
  const mutation = isEditing ? updateGoal : createGoal;

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    reset,
    formState: { errors },
  } = useForm<GoalFormValues>({
    resolver: zodResolver(goalSchema),
    defaultValues: { category: "Academic", priority: "Medium" },
  });

  useEffect(() => {
    if (!open) return;
    reset({
      title: goal?.title ?? "",
      description: goal?.description ?? "",
      category: goal?.category ?? "Academic",
      priority: goal?.priority ?? "Medium",
      targetDate: goal?.targetDate ?? "",
    });
  }, [open, goal, reset]);

  const onSubmit = (values: GoalFormValues) => {
    const input = {
      title: values.title,
      description: values.description || null,
      category: values.category as Goal["category"],
      priority: values.priority as Goal["priority"],
      targetDate: values.targetDate || null,
    };

    const action = isEditing ? updateGoal.mutateAsync({ id: goal.id, input }) : createGoal.mutateAsync(input);

    action.then(() => onOpenChange(false));
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{isEditing ? "Edit goal" : "New goal"}</DialogTitle>
          <DialogDescription>
            {isEditing ? "Update the details of this goal." : "Add a goal for the planner to build around."}
          </DialogDescription>
        </DialogHeader>

        <form className="flex flex-col gap-4" onSubmit={handleSubmit(onSubmit)}>
          <div className="flex flex-col gap-2">
            <Label htmlFor="title">Title</Label>
            <Input id="title" placeholder="Pass the AWS Solutions Architect exam" {...register("title")} />
            {errors.title && <p className="text-sm text-destructive">{errors.title.message}</p>}
          </div>

          <div className="flex flex-col gap-2">
            <Label htmlFor="description">Description</Label>
            <Textarea id="description" rows={3} placeholder="Optional details" {...register("description")} />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="flex flex-col gap-2">
              <Label>Category</Label>
              <Select value={watch("category")} onValueChange={(value) => value && setValue("category", value)}>
                <SelectTrigger className="w-full">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {GOAL_CATEGORIES.map((category) => (
                    <SelectItem key={category} value={category}>
                      {category}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="flex flex-col gap-2">
              <Label>Priority</Label>
              <Select value={watch("priority")} onValueChange={(value) => value && setValue("priority", value)}>
                <SelectTrigger className="w-full">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {GOAL_PRIORITIES.map((priority) => (
                    <SelectItem key={priority} value={priority}>
                      {priority}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="flex flex-col gap-2">
            <Label htmlFor="targetDate">Target date</Label>
            <Input id="targetDate" type="date" {...register("targetDate")} />
          </div>

          {mutation.isError && (
            <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
              Something went wrong. Please try again.
            </p>
          )}

          <DialogFooter>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Saving…" : isEditing ? "Save changes" : "Create goal"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
