import { Check, X } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { AnswerResult } from "@/lib/types/quiz";

export function AnswerReviewCard({ answer, index }: { answer: AnswerResult; index: number }) {
  return (
    <div className={cn("flex flex-col gap-3 rounded-xl border p-4", answer.isCorrect ? "border-primary/30 bg-primary/5" : "border-destructive/30 bg-destructive/5")}>
      <div className="flex items-start justify-between gap-3">
        <div className="flex items-start gap-2">
          <span
            className={cn(
              "mt-0.5 flex size-5 shrink-0 items-center justify-center rounded-full text-white",
              answer.isCorrect ? "bg-primary" : "bg-destructive",
            )}
          >
            {answer.isCorrect ? <Check className="size-3" /> : <X className="size-3" />}
          </span>
          <p className="text-sm font-medium">
            {index + 1}. {answer.questionText}
          </p>
        </div>
        <Badge variant="outline" className="shrink-0">
          {answer.topic}
        </Badge>
      </div>

      <div className="flex flex-col gap-1 pl-7 text-sm">
        <p className={cn(answer.isCorrect ? "text-foreground" : "text-destructive")}>
          Your answer: <span className="font-medium">{answer.userAnswer || "(no answer)"}</span>
        </p>
        {!answer.isCorrect && (
          <p className="text-foreground">
            Correct answer: <span className="font-medium">{answer.correctAnswer}</span>
          </p>
        )}
      </div>

      <p className="rounded-lg bg-muted/60 px-3 py-2 pl-7 text-sm text-muted-foreground">{answer.explanation}</p>
    </div>
  );
}
