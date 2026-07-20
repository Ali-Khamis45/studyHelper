import { Flame, Target, TrendingUp } from "lucide-react";

import { Card, CardContent } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import { cn } from "@/lib/utils";

interface PlannerStatsCardsProps {
  studyStreak: number;
  dailyFocusScore: number;
  dailyCompletionPercent: number;
}

export function PlannerStatsCards({ studyStreak, dailyFocusScore, dailyCompletionPercent }: PlannerStatsCardsProps) {
  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
      <Card>
        <CardContent className="flex items-center gap-3 py-2">
          <div className={cn("flex size-10 shrink-0 items-center justify-center rounded-full", studyStreak > 0 ? "bg-orange-500/10 text-orange-500" : "bg-muted text-muted-foreground")}>
            <Flame className="size-5" />
          </div>
          <div>
            <p className="text-2xl font-semibold tabular-nums">{studyStreak}</p>
            <p className="text-xs text-muted-foreground">Day streak</p>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="flex items-center gap-3 py-2">
          <div className="flex size-10 shrink-0 items-center justify-center rounded-full bg-primary/10 text-primary">
            <Target className="size-5" />
          </div>
          <div>
            <p className="text-2xl font-semibold tabular-nums">{dailyFocusScore}</p>
            <p className="text-xs text-muted-foreground">Focus score</p>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="flex flex-col gap-2 py-2">
          <div className="flex items-center gap-3">
            <div className="flex size-10 shrink-0 items-center justify-center rounded-full bg-emerald-500/10 text-emerald-500">
              <TrendingUp className="size-5" />
            </div>
            <div>
              <p className="text-2xl font-semibold tabular-nums">{dailyCompletionPercent.toFixed(0)}%</p>
              <p className="text-xs text-muted-foreground">Today&apos;s completion</p>
            </div>
          </div>
          <Progress value={dailyCompletionPercent} />
        </CardContent>
      </Card>
    </div>
  );
}
