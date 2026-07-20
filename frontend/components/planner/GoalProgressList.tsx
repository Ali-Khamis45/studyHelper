import Link from "next/link";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import type { Goal } from "@/lib/types/goals";

export function GoalProgressList({ goals }: { goals: Goal[] }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Active goals</CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        {goals.length === 0 ? (
          <p className="text-sm text-muted-foreground">
            No active goals yet.{" "}
            <Link href="/goals" className="underline underline-offset-4">
              Create one
            </Link>
            .
          </p>
        ) : (
          goals.map((goal) => (
            <div key={goal.id} className="flex flex-col gap-1.5">
              <div className="flex items-center justify-between gap-2 text-sm">
                <span className="truncate">{goal.title}</span>
                <span className="shrink-0 text-xs text-muted-foreground">{goal.progressPercent}%</span>
              </div>
              <Progress value={goal.progressPercent} />
            </div>
          ))
        )}
      </CardContent>
    </Card>
  );
}
