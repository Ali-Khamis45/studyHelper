"use client";

import { Textarea } from "@/components/ui/textarea";
import { cn } from "@/lib/utils";
import type { Question } from "@/lib/types/quiz";

export function QuestionCard({ question, value, onChange }: { question: Question; value: string; onChange: (value: string) => void }) {
  if (question.type === "MultipleChoice" || question.type === "TrueFalse") {
    return (
      <div className="flex flex-col gap-2">
        {(question.options ?? []).map((option) => (
          <button
            key={option}
            type="button"
            onClick={() => onChange(option)}
            className={cn(
              "flex items-center gap-3 rounded-xl border px-4 py-3 text-left text-sm transition-colors",
              value === option ? "border-primary bg-primary/5 font-medium" : "border-border hover:bg-muted",
            )}
          >
            <span
              className={cn(
                "flex size-4 shrink-0 items-center justify-center rounded-full border",
                value === option ? "border-primary bg-primary" : "border-muted-foreground/40",
              )}
            >
              {value === option && <span className="size-1.5 rounded-full bg-primary-foreground" />}
            </span>
            {option}
          </button>
        ))}
      </div>
    );
  }

  return (
    <Textarea
      value={value}
      onChange={(e) => onChange(e.target.value)}
      placeholder={question.type === "FillBlank" ? "Fill in the blank..." : "Your answer..."}
      rows={question.type === "FillBlank" ? 1 : 4}
      className="resize-none"
    />
  );
}
