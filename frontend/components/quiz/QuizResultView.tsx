"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { motion } from "framer-motion";
import { PartyPopper, RotateCcw, TrendingDown } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { AnswerReviewCard } from "@/components/quiz/AnswerReviewCard";
import { useRetryQuiz } from "@/lib/hooks/useQuiz";
import { cn } from "@/lib/utils";
import type { QuizAttemptResult } from "@/lib/types/quiz";

function useCountUp(target: number, durationMs = 900) {
  const [value, setValue] = useState(0);

  useEffect(() => {
    let raf: number;
    const start = performance.now();

    const tick = (now: number) => {
      const progress = Math.min(1, (now - start) / durationMs);
      setValue(Math.round(target * (1 - Math.pow(1 - progress, 3))));
      if (progress < 1) raf = requestAnimationFrame(tick);
    };

    raf = requestAnimationFrame(tick);
    return () => cancelAnimationFrame(raf);
  }, [target, durationMs]);

  return value;
}

export function QuizResultView({ result }: { result: QuizAttemptResult }) {
  const router = useRouter();
  const retryQuiz = useRetryQuiz();
  const animatedScore = useCountUp(result.score);

  const celebrate = result.score >= 80;
  const scoreColor = celebrate ? "text-gradient-brand" : result.score >= 50 ? "text-warning" : "text-destructive";

  const handleRetry = () => {
    retryQuiz.mutate(result.quizId, { onSuccess: () => router.push(`/quiz/${result.quizId}?retry=1`) });
  };

  return (
    <div className="mx-auto flex w-full max-w-2xl flex-col gap-6">
      <motion.div
        initial={{ opacity: 0, scale: 0.92 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.4, ease: "easeOut" }}
        className="glass ring-glass relative flex flex-col items-center gap-2 overflow-hidden rounded-3xl border border-border py-10 text-center"
      >
        {celebrate && <span className="pointer-events-none absolute inset-0 bg-gradient-brand-soft" aria-hidden="true" />}
        {celebrate && (
          <span className="relative mb-1 flex items-center gap-1.5 rounded-full bg-gradient-brand px-3 py-1 text-xs font-medium text-primary-foreground shadow-glow-primary">
            <PartyPopper className="size-3.5" />
            Great job!
          </span>
        )}
        <span className={cn("relative text-6xl font-bold tabular-nums", scoreColor)}>{animatedScore}%</span>
        <p className="relative text-sm text-muted-foreground">
          {result.correctCount} of {result.totalCount} correct
        </p>
        <div className="relative mt-2 flex items-center gap-3 text-xs text-muted-foreground">
          <span>Confidence: {Math.round(result.confidence * 100)}%</span>
          <span>·</span>
          <span>{result.quizTitle}</span>
        </div>
      </motion.div>

      {result.weakTopics.length > 0 && (
        <div className="glass flex flex-col gap-2 rounded-2xl border border-warning/30 bg-warning/5 p-4">
          <div className="flex items-center gap-2 text-sm font-medium text-warning">
            <TrendingDown className="size-4" />
            Weak topics from this attempt
          </div>
          <div className="flex flex-wrap gap-1.5">
            {result.weakTopics.map((topic) => (
              <Badge key={topic} variant="secondary">
                {topic}
              </Badge>
            ))}
          </div>
        </div>
      )}

      {result.recommendedTopics.length > 0 && (
        <div className="flex flex-col gap-2 rounded-xl border p-4">
          <p className="text-sm font-medium">Recommended next study topics</p>
          <div className="flex flex-wrap gap-1.5">
            {result.recommendedTopics.map((topic) => (
              <Badge key={topic} variant="outline">
                {topic}
              </Badge>
            ))}
          </div>
        </div>
      )}

      <div className="flex items-center gap-2">
        <Button onClick={handleRetry} disabled={retryQuiz.isPending} className="flex-1">
          <RotateCcw />
          Retry this quiz
        </Button>
        <Link href="/quiz" className="flex-1">
          <Button variant="outline" className="w-full">
            Back to quizzes
          </Button>
        </Link>
      </div>

      <div className="flex flex-col gap-3">
        <p className="text-sm font-semibold">Review</p>
        {result.answers.map((answer, i) => (
          <AnswerReviewCard key={answer.questionId} answer={answer} index={i} />
        ))}
      </div>
    </div>
  );
}
