import { CheckCircle2, MessageCircle, Target, Trophy } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { TimelineEvent } from "@/lib/types/analytics";

const ICONS: Record<string, typeof Target> = {
  GoalCreated: Target,
  TaskCompleted: CheckCircle2,
  QuizCompleted: Trophy,
  ConversationStarted: MessageCircle,
};

export function TimelineFeed({ events }: { events: TimelineEvent[] }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Activity Timeline</CardTitle>
      </CardHeader>
      <CardContent>
        {events.length === 0 ? (
          <p className="py-8 text-center text-sm text-muted-foreground">No activity yet.</p>
        ) : (
          <ul className="flex flex-col gap-3">
            {events.map((event, i) => {
              const Icon = ICONS[event.type] ?? Target;
              return (
                <li key={i} className="flex items-start gap-3 text-sm">
                  <span className="mt-0.5 flex size-6 shrink-0 items-center justify-center rounded-full bg-muted text-muted-foreground">
                    <Icon className="size-3.5" />
                  </span>
                  <div className="flex flex-col">
                    <span>{event.label}</span>
                    <span className="text-xs text-muted-foreground">{new Date(event.occurredAtUtc).toLocaleString()}</span>
                  </div>
                </li>
              );
            })}
          </ul>
        )}
      </CardContent>
    </Card>
  );
}
