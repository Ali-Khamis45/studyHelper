"use client";

import { useState } from "react";
import { Plus, Route } from "lucide-react";

import { CreateRoadmapWizard } from "@/components/roadmap/CreateRoadmapWizard";
import { RoadmapCard } from "@/components/roadmap/RoadmapCard";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useRoadmaps } from "@/lib/hooks/useRoadmap";

export default function RoadmapPage() {
  const { data: roadmaps, isLoading, isError } = useRoadmaps();
  const [wizardOpen, setWizardOpen] = useState(false);

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Learning Journeys</h1>
          <p className="text-muted-foreground">AI-generated roadmaps that evolve with your progress.</p>
        </div>
        <Button onClick={() => setWizardOpen(true)}>
          <Plus /> Create New Learning Journey
        </Button>
      </div>

      {isLoading && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-52 rounded-2xl" />
          ))}
        </div>
      )}

      {isError && (
        <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">Couldn&apos;t load your learning journeys. Try refreshing.</p>
      )}

      {roadmaps && roadmaps.length === 0 && (
        <div className="flex flex-col items-center gap-3 rounded-xl border border-dashed py-16 text-center">
          <span className="flex size-12 items-center justify-center rounded-2xl bg-accent text-accent-foreground">
            <Route className="size-5" />
          </span>
          <div>
            <p className="font-medium">No learning journeys yet</p>
            <p className="text-sm text-muted-foreground">Tell the AI what you want to become and it&apos;ll design your path.</p>
          </div>
          <Button onClick={() => setWizardOpen(true)}>
            <Plus /> Create New Learning Journey
          </Button>
        </div>
      )}

      {roadmaps && roadmaps.length > 0 && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {roadmaps.map((roadmap) => (
            <RoadmapCard key={roadmap.id} roadmap={roadmap} />
          ))}
        </div>
      )}

      <CreateRoadmapWizard open={wizardOpen} onOpenChange={setWizardOpen} />
    </div>
  );
}
