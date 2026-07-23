import { AlertTriangle } from "lucide-react";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import type { TopicMastery } from "@/lib/types/quiz";

export function WeakTopicsCard({ topics }: { topics: TopicMastery[] }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base">
          <AlertTriangle className="size-4 text-amber-500" />
          Weak topics
        </CardTitle>
        <CardDescription>Below 60% mastery — good candidates for a Review quiz.</CardDescription>
      </CardHeader>
      <CardContent>
        {topics.length === 0 ? (
          <p className="text-sm text-muted-foreground">No weak topics right now — nice work.</p>
        ) : (
          <ul className="flex flex-col gap-2">
            {topics.map((topic) => (
              <li key={topic.topic} className="flex items-center justify-between text-sm">
                <span>{topic.topic}</span>
                <span className="font-medium text-amber-600 dark:text-amber-400">{Math.round(topic.masteryScore * 100)}%</span>
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  );
}
