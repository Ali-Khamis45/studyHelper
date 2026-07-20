import { CalendarDays } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { UpcomingDeadline } from "@/lib/types/planner";

export function UpcomingDeadlines({ deadlines }: { deadlines: UpcomingDeadline[] }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base">
          <CalendarDays className="size-4" /> Upcoming deadlines
        </CardTitle>
      </CardHeader>
      <CardContent>
        {deadlines.length === 0 ? (
          <p className="text-sm text-muted-foreground">No goals with a target date coming up.</p>
        ) : (
          <ul className="flex flex-col gap-3">
            {deadlines.map((deadline) => (
              <li key={deadline.goalId} className="flex items-center justify-between gap-2 text-sm">
                <span className="truncate">{deadline.title}</span>
                <span className="shrink-0 text-xs text-muted-foreground">
                  {deadline.daysRemaining === 0 ? "Today" : `${deadline.daysRemaining}d`}
                </span>
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  );
}
