"use client";

import { AlertTriangle, RotateCcw, Sparkles } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useRegenerateInsights } from "@/lib/hooks/useAnalytics";
import type { Insights } from "@/lib/types/analytics";

export function InsightsPanel({ insights }: { insights: Insights | null }) {
  const regenerate = useRegenerateInsights();

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle className="flex items-center gap-2 text-base">
          <Sparkles className="size-4 text-primary" />
          AI Insights
        </CardTitle>
        <Button variant="ghost" size="icon-sm" onClick={() => regenerate.mutate()} disabled={regenerate.isPending} title="Regenerate">
          <RotateCcw className={regenerate.isPending ? "animate-spin" : ""} />
        </Button>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        {!insights ? (
          <p className="text-sm text-muted-foreground">
            {regenerate.isPending ? "Generating your report…" : "Not enough data yet, or generation is temporarily unavailable."}
          </p>
        ) : (
          <>
            <div className="flex flex-col gap-1">
              <span className="text-xs font-semibold text-muted-foreground">This Week</span>
              <p className="text-sm">{insights.weeklySummary}</p>
            </div>
            <div className="flex flex-col gap-1">
              <span className="text-xs font-semibold text-muted-foreground">This Month</span>
              <p className="text-sm">{insights.monthlySummary}</p>
            </div>

            {insights.strengths.length > 0 && (
              <div className="flex flex-col gap-1.5">
                <span className="text-xs font-semibold text-muted-foreground">Strengths</span>
                <ul className="flex flex-col gap-1">
                  {insights.strengths.map((s, i) => (
                    <li key={i} className="text-sm">
                      • {s}
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {insights.weaknesses.length > 0 && (
              <div className="flex flex-col gap-1.5">
                <span className="text-xs font-semibold text-muted-foreground">Weaknesses</span>
                <ul className="flex flex-col gap-1">
                  {insights.weaknesses.map((w, i) => (
                    <li key={i} className="text-sm">
                      • {w}
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {insights.recommendedFocusAreas.length > 0 && (
              <div className="flex flex-col gap-1.5">
                <span className="text-xs font-semibold text-muted-foreground">Recommended Focus</span>
                <div className="flex flex-wrap gap-1.5">
                  {insights.recommendedFocusAreas.map((topic) => (
                    <Badge key={topic} variant="secondary">
                      {topic}
                    </Badge>
                  ))}
                </div>
              </div>
            )}

            <div className="flex items-start gap-2 rounded-lg border border-amber-500/30 bg-amber-500/5 px-3 py-2 text-sm text-amber-700 dark:text-amber-400">
              <AlertTriangle className="mt-0.5 size-4 shrink-0" />
              {insights.riskDetection}
            </div>

            {insights.suggestedScheduleImprovements.length > 0 && (
              <div className="flex flex-col gap-1.5">
                <span className="text-xs font-semibold text-muted-foreground">Schedule Suggestions</span>
                <ul className="flex flex-col gap-1">
                  {insights.suggestedScheduleImprovements.map((s, i) => (
                    <li key={i} className="text-sm">
                      • {s}
                    </li>
                  ))}
                </ul>
              </div>
            )}

            <span className="text-xs text-muted-foreground">Generated {new Date(insights.generatedAtUtc).toLocaleString()}</span>
          </>
        )}
      </CardContent>
    </Card>
  );
}
