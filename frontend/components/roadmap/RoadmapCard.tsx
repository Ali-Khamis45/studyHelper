"use client";

import Link from "next/link";
import { MoreHorizontal, Route, Trash2 } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Progress } from "@/components/ui/progress";
import { useDeleteRoadmap } from "@/lib/hooks/useRoadmap";
import type { RoadmapSummary } from "@/lib/types/roadmap";

const DIFFICULTY_VARIANT: Record<RoadmapSummary["difficulty"], "default" | "secondary" | "outline"> = {
  Advanced: "default",
  Intermediate: "secondary",
  Beginner: "outline",
};

export function RoadmapCard({ roadmap }: { roadmap: RoadmapSummary }) {
  const deleteRoadmap = useDeleteRoadmap();

  return (
    <Card>
      <CardHeader>
        <div className="flex items-start justify-between gap-2">
          <Link href={`/roadmap/${roadmap.id}`} className="min-w-0 flex-1">
            <CardTitle className="flex items-center gap-2 text-base">
              <span className="flex size-7 shrink-0 items-center justify-center rounded-lg bg-gradient-brand text-primary-foreground">
                <Route className="size-3.5" />
              </span>
              <span className="truncate">{roadmap.title}</span>
            </CardTitle>
          </Link>
          <DropdownMenu>
            <DropdownMenuTrigger render={<Button variant="ghost" size="icon-sm" aria-label="Roadmap actions" />}>
              <MoreHorizontal />
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem variant="destructive" onClick={() => deleteRoadmap.mutate(roadmap.id)}>
                <Trash2 /> Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
        <p className="text-sm text-muted-foreground">{roadmap.careerGoal}</p>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        <div className="flex flex-wrap items-center gap-2">
          <Badge variant={DIFFICULTY_VARIANT[roadmap.difficulty]}>{roadmap.difficulty}</Badge>
          <Badge variant="outline">{roadmap.estimatedWeeks} weeks</Badge>
          {roadmap.status !== "Active" && <Badge variant="secondary">{roadmap.status}</Badge>}
        </div>

        <div className="flex flex-col gap-1.5">
          <div className="flex items-center justify-between text-xs text-muted-foreground">
            <span>Progress</span>
            <span>
              {roadmap.completedTopicCount}/{roadmap.totalTopicCount} topics &middot; {roadmap.progressPercent}%
            </span>
          </div>
          <Progress value={roadmap.progressPercent} />
        </div>

        <Link href={`/roadmap/${roadmap.id}`}>
          <Button variant="outline" size="sm" className="w-full">
            Continue journey
          </Button>
        </Link>
      </CardContent>
    </Card>
  );
}
