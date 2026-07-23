"use client";

import Link from "next/link";
import { Trash2 } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useDeleteQuiz } from "@/lib/hooks/useQuiz";
import type { QuizSummary } from "@/lib/types/quiz";

export function QuizCard({ quiz }: { quiz: QuizSummary }) {
  const deleteQuiz = useDeleteQuiz();

  const handleDelete = (event: React.MouseEvent) => {
    event.preventDefault();
    if (!window.confirm(`Delete "${quiz.title}"? This can't be undone.`)) return;
    deleteQuiz.mutate(quiz.id);
  };

  return (
    <Link href={`/quiz/${quiz.id}`} className="group block">
      <Card className="h-full gap-3 py-5 transition-all duration-200 ease-out hover:-translate-y-0.5 hover:border-primary/50 hover:shadow-md">
        <CardHeader className="flex flex-row items-start justify-between gap-2">
          <CardTitle className="text-base leading-snug">{quiz.title}</CardTitle>
          <Button variant="ghost" size="icon-xs" className="shrink-0 opacity-0 group-hover:opacity-100" onClick={handleDelete}>
            <Trash2 />
          </Button>
        </CardHeader>
        <CardContent className="flex flex-col gap-3">
          <div className="flex flex-wrap items-center gap-1.5">
            <Badge variant="secondary">{quiz.topic}</Badge>
            <Badge variant="outline">{quiz.difficulty}</Badge>
            <Badge variant="outline">{quiz.questionCount} questions</Badge>
          </div>
          {quiz.latestAttemptScore !== null ? (
            <p className="text-sm text-muted-foreground">
              Last score: <span className="font-medium text-foreground">{quiz.latestAttemptScore}%</span>
            </p>
          ) : (
            <p className="text-sm text-muted-foreground">Not attempted yet</p>
          )}
        </CardContent>
      </Card>
    </Link>
  );
}
