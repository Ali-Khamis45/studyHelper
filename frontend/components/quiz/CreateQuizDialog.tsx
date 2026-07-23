"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";

import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useGenerateQuiz } from "@/lib/hooks/useQuiz";
import type { DifficultyValue, GenerateQuizInput, QuestionTypeValue, QuizTypeValue } from "@/lib/types/quiz";
import { cn } from "@/lib/utils";

const QUESTION_TYPES: { value: QuestionTypeValue; label: string }[] = [
  { value: "MultipleChoice", label: "Multiple Choice" },
  { value: "TrueFalse", label: "True / False" },
  { value: "ShortAnswer", label: "Short Answer" },
  { value: "FillBlank", label: "Fill in the Blank" },
];

export function CreateQuizDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (open: boolean) => void }) {
  const router = useRouter();
  const generateQuiz = useGenerateQuiz();

  const [topic, setTopic] = useState("");
  const [difficulty, setDifficulty] = useState<DifficultyValue>("Medium");
  const [quizType, setQuizType] = useState<QuizTypeValue>("Standard");
  const [questionCount, setQuestionCount] = useState(5);
  const [questionTypes, setQuestionTypes] = useState<QuestionTypeValue[]>(["MultipleChoice", "TrueFalse"]);

  const toggleType = (type: QuestionTypeValue) => {
    setQuestionTypes((prev) => (prev.includes(type) ? prev.filter((t) => t !== type) : [...prev, type]));
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (questionTypes.length === 0) return;

    const input: GenerateQuizInput = {
      topic: quizType === "Review" ? null : topic.trim(),
      goalId: null,
      difficulty,
      questionTypes,
      questionCount,
      quizType,
    };

    generateQuiz.mutate(input, {
      onSuccess: (quiz) => {
        onOpenChange(false);
        router.push(`/quiz/${quiz.id}`);
      },
    });
  };

  const canSubmit = questionTypes.length > 0 && (quizType === "Review" || topic.trim().length > 0) && !generateQuiz.isPending;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>New quiz</DialogTitle>
          <DialogDescription>AI-generated, grounded in your goals and progress so far.</DialogDescription>
        </DialogHeader>

        <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
          <div className="flex flex-col gap-2">
            <Label>Mode</Label>
            <Select value={quizType} onValueChange={(v) => v && setQuizType(v as QuizTypeValue)}>
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Standard">Standard — a fixed set on a topic you choose</SelectItem>
                <SelectItem value="Adaptive">Adaptive — biased toward your weak areas</SelectItem>
                <SelectItem value="Review">Review — generated entirely from your weak topics</SelectItem>
              </SelectContent>
            </Select>
          </div>

          {quizType !== "Review" && (
            <div className="flex flex-col gap-2">
              <Label htmlFor="topic">Topic</Label>
              <Input id="topic" placeholder="e.g. Cellular respiration" value={topic} onChange={(e) => setTopic(e.target.value)} />
            </div>
          )}

          <div className="grid grid-cols-2 gap-4">
            <div className="flex flex-col gap-2">
              <Label>Difficulty</Label>
              <Select value={difficulty} onValueChange={(v) => v && setDifficulty(v as DifficultyValue)}>
                <SelectTrigger className="w-full">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Easy">Easy</SelectItem>
                  <SelectItem value="Medium">Medium</SelectItem>
                  <SelectItem value="Hard">Hard</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="flex flex-col gap-2">
              <Label htmlFor="questionCount">Questions</Label>
              <Input
                id="questionCount"
                type="number"
                min={1}
                max={15}
                value={questionCount}
                onChange={(e) => setQuestionCount(Math.min(15, Math.max(1, Number(e.target.value) || 1)))}
              />
            </div>
          </div>

          <div className="flex flex-col gap-2">
            <Label>Question types</Label>
            <div className="flex flex-wrap gap-2">
              {QUESTION_TYPES.map(({ value, label }) => (
                <button
                  key={value}
                  type="button"
                  onClick={() => toggleType(value)}
                  className={cn(
                    "rounded-full border px-3 py-1 text-xs font-medium transition-colors",
                    questionTypes.includes(value)
                      ? "border-primary bg-primary text-primary-foreground"
                      : "border-border bg-background text-muted-foreground hover:bg-muted",
                  )}
                >
                  {label}
                </button>
              ))}
            </div>
          </div>

          {generateQuiz.isError && (
            <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
              {(generateQuiz.error as { body?: { title?: string } })?.body?.title ?? "Couldn't generate a quiz. Please try again."}
            </p>
          )}

          <DialogFooter>
            <Button type="submit" disabled={!canSubmit}>
              {generateQuiz.isPending ? "Generating…" : "Generate quiz"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
