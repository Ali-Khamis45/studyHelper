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
              "flex items-center gap-3 rounded-2xl border px-4 py-3.5 text-left text-sm transition-all duration-200",
              value === option
                ? "border-transparent bg-gradient-brand font-medium text-primary-foreground shadow-glow-primary"
                : "glass border-border hover:-translate-y-0.5 hover:border-primary/40",
            )}
          >
            <span
              className={cn(
                "flex size-4 shrink-0 items-center justify-center rounded-full border-2",
                value === option ? "border-primary-foreground" : "border-muted-foreground/40",
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
