import Link from "next/link";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { QuizHistoryItem } from "@/lib/types/quiz";

function scoreColor(score: number | null) {
  if (score === null) return "text-muted-foreground";
  if (score >= 80) return "text-primary";
  if (score >= 50) return "text-amber-500";
  return "text-destructive";
}

export function QuizHistoryCard({ history }: { history: QuizHistoryItem[] }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Recent attempts</CardTitle>
      </CardHeader>
      <CardContent>
        {history.length === 0 ? (
          <p className="text-sm text-muted-foreground">No quiz attempts yet.</p>
        ) : (
          <ul className="flex flex-col divide-y">
            {history.map((item) => (
              <li key={item.attemptId} className="flex items-center justify-between py-2.5 text-sm">
                <Link href={`/quiz/attempts/${item.attemptId}`} className="flex flex-col hover:underline">
                  <span className="font-medium">{item.quizTitle}</span>
                  <span className="text-xs text-muted-foreground">
                    {item.topic} · {new Date(item.startedAtUtc).toLocaleDateString()}
                  </span>
                </Link>
                <span className={`font-semibold tabular-nums ${scoreColor(item.score)}`}>{item.score !== null ? `${item.score}%` : "—"}</span>
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  );
}
