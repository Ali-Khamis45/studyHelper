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
          <div className={cn("flex size-11 shrink-0 items-center justify-center rounded-full", studyStreak > 0 ? "bg-gradient-to-br from-orange-400 to-amber-500 text-white shadow-[0_6px_16px_-4px_rgba(249,115,22,0.5)]" : "bg-muted text-muted-foreground")}>
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
          <div className="flex size-11 shrink-0 items-center justify-center rounded-full bg-gradient-brand text-primary-foreground shadow-glow-primary">
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
            <div className="flex size-11 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-emerald-400 to-teal-500 text-white shadow-[0_6px_16px_-4px_rgba(16,185,129,0.5)]">
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
