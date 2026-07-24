"use client";

import { use } from "react";
import { useRouter } from "next/navigation";
import { Clock, Route, Target, Trash2 } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Skeleton } from "@/components/ui/skeleton";
import { TopicNode } from "@/components/roadmap/TopicNode";
import { useDeleteRoadmap, useRoadmapDetail } from "@/lib/hooks/useRoadmap";

const DIFFICULTY_VARIANT: Record<string, "default" | "secondary" | "outline"> = {
  Advanced: "default",
  Intermediate: "secondary",
  Beginner: "outline",
};

export default function RoadmapDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();
  const { data: roadmap, isLoading, isError } = useRoadmapDetail(id);
  const deleteRoadmap = useDeleteRoadmap();

  const handleDelete = () => {
    if (!window.confirm("Delete this learning journey? This can't be undone.")) return;
    deleteRoadmap.mutate(id, { onSuccess: () => router.push("/roadmap") });
  };

  if (isLoading) {
    return (
      <div className="mx-auto flex w-full max-w-4xl flex-col gap-6">
        <Skeleton className="h-40 rounded-2xl" />
        <Skeleton className="h-24 rounded-2xl" />
        <Skeleton className="h-24 rounded-2xl" />
      </div>
    );
  }

  if (isError || !roadmap) {
    return <p className="mx-auto max-w-2xl text-center text-sm text-muted-foreground">This learning journey couldn&apos;t be found.</p>;
  }

  return (
    <div className="mx-auto flex w-full max-w-4xl flex-col gap-6">
      <div className="ring-glass relative overflow-hidden rounded-2xl bg-gradient-brand p-6 text-primary-foreground sm:p-8">
        <div
          className="pointer-events-none absolute inset-0 opacity-60"
          style={{
            backgroundImage:
              "radial-gradient(circle at 85% 0%, color-mix(in oklch, var(--brand-cyan) 45%, transparent) 0%, transparent 55%)",
          }}
        />
        <div className="relative flex flex-col gap-4">
          <div className="flex items-start justify-between gap-4">
            <div className="flex items-start gap-3">
              <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-primary-foreground/15 backdrop-blur">
                <Route className="size-5" />
              </span>
              <div>
                <span className="text-xs font-medium text-primary-foreground/70">{roadmap.careerGoal}</span>
                <h1 className="text-2xl font-bold tracking-tight">{roadmap.title}</h1>
              </div>
            </div>
            <Button variant="outline" size="icon" className="border-primary-foreground/20 bg-primary-foreground/10 text-primary-foreground hover:bg-primary-foreground/20" onClick={handleDelete}>
              <Trash2 />
            </Button>
          </div>

          <p className="text-sm text-primary-foreground/80">{roadmap.description}</p>

          <div className="flex flex-wrap items-center gap-2">
            <Badge variant={DIFFICULTY_VARIANT[roadmap.difficulty] ?? "outline"}>{roadmap.difficulty}</Badge>
            <span className="flex items-center gap-1 text-xs text-primary-foreground/80">
              <Clock className="size-3" /> {roadmap.estimatedWeeks} weeks estimated
            </span>
            <span className="flex items-center gap-1 text-xs text-primary-foreground/80">
              <Target className="size-3" /> {roadmap.remainingEstimatedHours}h remaining
            </span>
          </div>

          <div className="flex flex-col gap-1.5">
            <div className="flex items-center justify-between text-xs text-primary-foreground/80">
              <span>Progress</span>
              <span>
                {roadmap.completedTopicCount}/{roadmap.totalTopicCount} topics &middot; {roadmap.progressPercent}%
              </span>
            </div>
            <Progress value={roadmap.progressPercent} className="[&_[data-slot=progress-track]]:bg-primary-foreground/20" />
          </div>
        </div>
      </div>

      <div className="flex flex-col gap-3">
        {roadmap.sections.map((section) => (
          <TopicNode key={section.id} topic={section} roadmapId={roadmap.id} depth={0} />
        ))}
      </div>
    </div>
  );
}
