"use client";

import { use, useState } from "react";

import { Skeleton } from "@/components/ui/skeleton";
import { QuizTakingView } from "@/components/quiz/QuizTakingView";
import { QuizResultView } from "@/components/quiz/QuizResultView";
import { useQuiz } from "@/lib/hooks/useQuiz";
import type { QuizAttemptResult } from "@/lib/types/quiz";

export default function QuizTakePage({ params }: { params: Promise<{ quizId: string }> }) {
  const { quizId } = use(params);
  const { data: quiz, isLoading, isError } = useQuiz(quizId);
  const [result, setResult] = useState<QuizAttemptResult | null>(null);

  if (isLoading) {
    return (
      <div className="mx-auto flex w-full max-w-2xl flex-col gap-6">
        <Skeleton className="h-8 w-2/3" />
        <Skeleton className="h-64 rounded-2xl" />
        <Skeleton className="h-10 w-full" />
      </div>
    );
  }

  if (isError || !quiz) {
    return <p className="mx-auto max-w-2xl text-center text-sm text-muted-foreground">This quiz couldn&apos;t be found.</p>;
  }

  if (result) return <QuizResultView result={result} />;

  return <QuizTakingView quiz={quiz} onComplete={setResult} />;
}
