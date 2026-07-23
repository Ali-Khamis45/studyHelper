"use client";

import { useState } from "react";
import { Plus } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { CreateQuizDialog } from "@/components/quiz/CreateQuizDialog";
import { QuizCard } from "@/components/quiz/QuizCard";
import { MasteryChart } from "@/components/quiz/MasteryChart";
import { WeakTopicsCard } from "@/components/quiz/WeakTopicsCard";
import { QuizHistoryCard } from "@/components/quiz/QuizHistoryCard";
import { useQuizHistory, useQuizzes, useTopicMastery, useWeakTopics } from "@/lib/hooks/useQuiz";

export default function QuizPage() {
  const [createOpen, setCreateOpen] = useState(false);
  const { data: quizzes, isLoading: quizzesLoading } = useQuizzes(1, 12);
  const { data: mastery, isLoading: masteryLoading } = useTopicMastery();
  const { data: weakTopics } = useWeakTopics();
  const { data: history } = useQuizHistory(1, 8);

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-8">
      <div className="flex items-center justify-between">
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-semibold tracking-tight">Quiz</h1>
          <p className="text-muted-foreground">AI-generated practice tied to your real goals and progress.</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus />
          New quiz
        </Button>
      </div>

      <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
        <div className="flex flex-col gap-4">
          <h2 className="text-sm font-semibold text-muted-foreground">Your quizzes</h2>
          {quizzesLoading ? (
            <div className="grid gap-4 sm:grid-cols-2">
              {Array.from({ length: 4 }).map((_, i) => (
                <Skeleton key={i} className="h-32 rounded-xl" />
              ))}
            </div>
          ) : quizzes && quizzes.items.length > 0 ? (
            <div className="grid gap-4 sm:grid-cols-2">
              {quizzes.items.map((quiz) => (
                <QuizCard key={quiz.id} quiz={quiz} />
              ))}
            </div>
          ) : (
            <div className="flex flex-col items-center gap-3 rounded-xl border border-dashed py-12 text-center">
              <p className="text-sm text-muted-foreground">No quizzes yet — generate your first one.</p>
              <Button variant="outline" onClick={() => setCreateOpen(true)}>
                <Plus />
                New quiz
              </Button>
            </div>
          )}
        </div>

        <div className="flex flex-col gap-4">
          {masteryLoading ? <Skeleton className="h-48 rounded-xl" /> : <MasteryChart mastery={mastery ?? []} />}
          <WeakTopicsCard topics={weakTopics ?? []} />
          {history && <QuizHistoryCard history={history.items} />}
        </div>
      </div>

      <CreateQuizDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  );
}
