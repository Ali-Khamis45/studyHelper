"use client";

import { useState } from "react";
import { History } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { useRecommendationHistory } from "@/lib/hooks/usePlanner";
import type { PlannerRecommendation } from "@/lib/types/planner";
import { cn } from "@/lib/utils";

/// Lets the student browse and expand past recommendations (including invalidated ones — nothing
/// is ever deleted) to see how the plan evolved; expanding two at once is how "comparison" works
/// here — a real diff view isn't worth the complexity for what it would add on top of this.
export function RecommendationHistoryCard() {
  const { data: history, isLoading } = useRecommendationHistory();
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set());

  const toggle = (id: string) => {
    setExpandedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base">
          <History className="size-4" /> Recommendation history
        </CardTitle>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="flex flex-col gap-2">
            <Skeleton className="h-10 rounded-md" />
            <Skeleton className="h-10 rounded-md" />
          </div>
        ) : !history || history.length === 0 ? (
          <p className="text-sm text-muted-foreground">No recommendations generated yet.</p>
        ) : (
          <ul className="flex flex-col gap-2">
            {history.map((entry) => (
              <HistoryEntry key={entry.id} entry={entry} expanded={expandedIds.has(entry.id)} onToggle={() => toggle(entry.id)} />
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  );
}

function HistoryEntry({
  entry,
  expanded,
  onToggle,
}: {
  entry: PlannerRecommendation;
  expanded: boolean;
  onToggle: () => void;
}) {
  const isActive = new Date(entry.expiresAt) > new Date();

  return (
    <li className="rounded-md border">
      <button type="button" onClick={onToggle} className="flex w-full items-center justify-between gap-2 px-3 py-2 text-left text-sm hover:bg-muted/50">
        <span className="flex flex-wrap items-center gap-2">
          <span className="font-medium">{new Date(entry.date).toLocaleDateString(undefined, { month: "short", day: "numeric", year: "numeric" })}</span>
          <Badge variant={isActive ? "default" : "outline"}>{isActive ? "Active" : "Superseded"}</Badge>
          <span className="text-xs text-muted-foreground">{Math.round(entry.confidenceScore * 100)}% confidence</span>
        </span>
        <span className="shrink-0 text-xs text-muted-foreground">{new Date(entry.generatedAt).toLocaleTimeString(undefined, { hour: "numeric", minute: "2-digit" })}</span>
      </button>
      <div className={cn("grid transition-all", expanded ? "grid-rows-[1fr]" : "grid-rows-[0fr]")}>
        <div className="overflow-hidden">
          <div className="flex flex-col gap-2 border-t px-3 py-3 text-sm">
            <p>{entry.recommendation}</p>
            <p className="text-xs text-muted-foreground italic">{entry.recommendationReason}</p>
            <p className="text-xs text-muted-foreground">
              {entry.provider} / {entry.modelUsed} · {entry.generationTimeMs}ms
            </p>
          </div>
        </div>
      </div>
    </li>
  );
}
