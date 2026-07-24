"use client";

import { useEffect, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import { ChevronLeft, ChevronRight, Timer } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { QuestionCard } from "@/components/quiz/QuestionCard";
import { useSubmitQuiz } from "@/lib/hooks/useQuiz";
import type { QuizAttemptResult, QuizDetail } from "@/lib/types/quiz";

function formatElapsed(seconds: number) {
  const m = Math.floor(seconds / 60);
  const s = seconds % 60;
  return `${m}:${s.toString().padStart(2, "0")}`;
}

export function QuizTakingView({ quiz, onComplete }: { quiz: QuizDetail; onComplete: (result: QuizAttemptResult) => void }) {
  const [index, setIndex] = useState(0);
  const [answers, setAnswers] = useState<Record<string, string>>({});
  const [elapsed, setElapsed] = useState(0);
  const submitQuiz = useSubmitQuiz();

  useEffect(() => {
    const interval = setInterval(() => setElapsed((prev) => prev + 1), 1000);
    return () => clearInterval(interval);
  }, []);

  const question = quiz.questions[index];
  const answeredCount = Object.values(answers).filter((a) => a.trim().length > 0).length;
  const progressPercent = (answeredCount / quiz.questions.length) * 100;
  const isLast = index === quiz.questions.length - 1;

  const handleSubmit = () => {
    const payload = quiz.questions.map((q) => ({ questionId: q.id, answer: answers[q.id]?.trim() ?? "" }));
    submitQuiz.mutate({ quizId: quiz.id, answers: payload }, { onSuccess: onComplete });
  };

  return (
    <div className="mx-auto flex w-full max-w-2xl flex-col gap-6">
      <div className="flex items-center justify-between">
        <div className="flex flex-col gap-1">
          <h1 className="text-xl font-semibold tracking-tight">{quiz.title}</h1>
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Badge variant="secondary">{quiz.difficulty}</Badge>
            <span>
              Question {index + 1} of {quiz.questions.length}
            </span>
          </div>
        </div>
        <div className="flex items-center gap-1.5 rounded-full bg-muted px-3 py-1.5 text-sm font-medium tabular-nums">
          <Timer className="size-3.5" />
          {formatElapsed(elapsed)}
        </div>
      </div>

      <Progress value={progressPercent} />

      <AnimatePresence mode="wait">
        <motion.div
          key={question.id}
          initial={{ opacity: 0, x: 16 }}
          animate={{ opacity: 1, x: 0 }}
          exit={{ opacity: 0, x: -16 }}
          transition={{ duration: 0.25, ease: "easeOut" }}
          className="glass ring-glass flex flex-col gap-4 rounded-2xl border border-border p-6 sm:p-8"
        >
          <div className="flex items-center gap-2">
            <Badge variant="outline">{question.topic}</Badge>
            <Badge variant="outline">{question.difficulty}</Badge>
          </div>
          <p className="text-xl leading-relaxed font-semibold text-balance">{question.text}</p>
          <QuestionCard question={question} value={answers[question.id] ?? ""} onChange={(value) => setAnswers((prev) => ({ ...prev, [question.id]: value }))} />
        </motion.div>
      </AnimatePresence>

      <div className="flex items-center justify-between">
        <Button type="button" variant="outline" onClick={() => setIndex((i) => Math.max(0, i - 1))} disabled={index === 0}>
          <ChevronLeft />
          Previous
        </Button>

        <div className="flex items-center gap-1">
          {quiz.questions.map((q, i) => (
            <button
              key={q.id}
              type="button"
              onClick={() => setIndex(i)}
              aria-label={`Go to question ${i + 1}`}
              className={`size-2 rounded-full transition-colors ${
                i === index ? "bg-primary" : answers[q.id]?.trim() ? "bg-primary/40" : "bg-muted-foreground/30"
              }`}
            />
          ))}
        </div>

        {isLast ? (
          <Button type="button" onClick={handleSubmit} disabled={submitQuiz.isPending}>
            {submitQuiz.isPending ? "Grading…" : "Submit quiz"}
          </Button>
        ) : (
          <Button type="button" onClick={() => setIndex((i) => Math.min(quiz.questions.length - 1, i + 1))}>
            Next
            <ChevronRight />
          </Button>
        )}
      </div>

      {submitQuiz.isError && (
        <p className="rounded-md bg-destructive/10 px-3 py-2 text-center text-sm text-destructive">Couldn&apos;t submit your answers. Please try again.</p>
      )}
    </div>
  );
}
