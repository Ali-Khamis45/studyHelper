"use client";

import Link from "next/link";
import { Route } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button, buttonVariants } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import { Skeleton } from "@/components/ui/skeleton";
import { useRoadmaps } from "@/lib/hooks/useRoadmap";

export function RoadmapWidget() {
  const { data: roadmaps, isLoading } = useRoadmaps();

  if (isLoading) return <Skeleton className="h-40 rounded-xl" />;

  const active = roadmaps?.find((r) => r.status === "Active");

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base">
          <Route className="size-4 text-primary" />
          Learning Journey
        </CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-3">
        {!active ? (
          <>
            <p className="text-sm text-muted-foreground">No active learning journey yet — tell the AI what you want to become.</p>
            <Link href="/roadmap" className={buttonVariants({ variant: "outline", size: "sm", className: "w-fit" })}>
              Create one
            </Link>
          </>
        ) : (
          <>
            <div className="flex items-start justify-between gap-2">
              <div>
                <p className="text-sm font-medium">{active.title}</p>
                <p className="text-xs text-muted-foreground">{active.careerGoal}</p>
              </div>
              <Badge variant="outline">{active.difficulty}</Badge>
            </div>
            <div className="flex flex-col gap-1.5">
              <div className="flex items-center justify-between text-xs text-muted-foreground">
                <span>{active.completedTopicCount}/{active.totalTopicCount} topics</span>
                <span>{active.progressPercent}%</span>
              </div>
              <Progress value={active.progressPercent} />
            </div>
            <Link href={`/roadmap/${active.id}`}>
              <Button variant="outline" size="sm" className="w-full">
                Continue journey
              </Button>
            </Link>
          </>
        )}
      </CardContent>
    </Card>
  );
}
