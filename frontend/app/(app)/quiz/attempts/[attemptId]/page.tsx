"use client";

import { use } from "react";

import { Skeleton } from "@/components/ui/skeleton";
import { QuizResultView } from "@/components/quiz/QuizResultView";
import { useAttempt } from "@/lib/hooks/useQuiz";

export default function QuizAttemptReviewPage({ params }: { params: Promise<{ attemptId: string }> }) {
  const { attemptId } = use(params);
  const { data: attempt, isLoading, isError } = useAttempt(attemptId);

  if (isLoading) {
    return (
      <div className="mx-auto flex w-full max-w-2xl flex-col gap-6">
        <Skeleton className="h-40 rounded-2xl" />
        <Skeleton className="h-24 rounded-xl" />
        <Skeleton className="h-24 rounded-xl" />
      </div>
    );
  }

  if (isError || !attempt) {
    return <p className="mx-auto max-w-2xl text-center text-sm text-muted-foreground">This attempt couldn&apos;t be found.</p>;
  }

  return <QuizResultView result={attempt} />;
}
