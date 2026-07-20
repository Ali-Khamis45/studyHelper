"use client";

import { RefreshCw, Sparkles } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useStreamRecommendation } from "@/lib/hooks/usePlanner";
import type { PlannerRecommendation } from "@/lib/types/planner";
import { cn } from "@/lib/utils";

const SECTIONS: { key: keyof PlannerRecommendation; label: string }[] = [
  { key: "situationAnalysis", label: "Situation" },
  { key: "goalAlignment", label: "Why this focus" },
  { key: "evidence", label: "Evidence" },
  { key: "recommendation", label: "Plan" },
];

export function DailyLoopCard({ recommendation }: { recommendation: PlannerRecommendation | null }) {
  const stream = useStreamRecommendation();

  if (stream.isStreaming) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <Sparkles className="size-4 animate-pulse" /> Generating today&apos;s plan…
          </CardTitle>
          <CardDescription>Streaming live from the AI mentor.</CardDescription>
        </CardHeader>
        <CardContent>
          <pre className="max-h-40 overflow-y-auto rounded-md bg-muted p-3 text-xs whitespace-pre-wrap text-muted-foreground">
            {stream.partialText || "Connecting…"}
          </pre>
        </CardContent>
      </Card>
    );
  }

  if (!recommendation) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-base">No recommendation yet</CardTitle>
          <CardDescription>Generate today&apos;s plan from your active goals.</CardDescription>
        </CardHeader>
        <CardContent>
          <Button onClick={() => stream.start()} disabled={stream.isStreaming}>
            <Sparkles /> Generate today&apos;s plan
          </Button>
          {stream.error && (
            <p className="mt-3 rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{stream.error}</p>
          )}
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-start justify-between gap-2">
          <div>
            <CardTitle className="text-base">Today&apos;s focus</CardTitle>
            <CardDescription>{recommendation.immediateNextAction}</CardDescription>
          </div>
          <Button variant="outline" size="sm" onClick={() => stream.start()} disabled={stream.isStreaming}>
            <RefreshCw className={cn(stream.isStreaming && "animate-spin")} /> Regenerate
          </Button>
        </div>
      </CardHeader>
      <CardContent className="flex flex-col gap-3">
        {SECTIONS.map(({ key, label }) => (
          <div key={key}>
            <p className="text-xs font-medium tracking-wide text-muted-foreground uppercase">{label}</p>
            <p className="text-sm">{recommendation[key] as string}</p>
          </div>
        ))}

        <div className="flex flex-col gap-1 border-t pt-3">
          <p className="text-xs text-muted-foreground italic">{recommendation.recommendationReason}</p>
          <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-muted-foreground">
            <span>Confidence: {Math.round(recommendation.confidenceScore * 100)}%</span>
            <span>
              {recommendation.provider} / {recommendation.modelUsed}
            </span>
            <span>{recommendation.generationTimeMs}ms</span>
          </div>
        </div>

        {stream.error && (
          <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{stream.error}</p>
        )}
      </CardContent>
    </Card>
  );
}
