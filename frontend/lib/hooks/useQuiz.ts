"use client";

import { useCallback, useRef, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  deleteQuiz,
  generateQuiz,
  getAttempt,
  getQuiz,
  getQuizHistory,
  getQuizzes,
  getTopicMastery,
  getWeakTopics,
  retryQuiz,
  streamGenerateQuiz,
  submitQuiz,
} from "@/lib/api/quiz";
import type { GenerateQuizInput, QuizDetail, SubmittedAnswer } from "@/lib/types/quiz";

export function useQuizzes(page = 1, pageSize = 20) {
  return useQuery({ queryKey: ["quiz", "list", page, pageSize], queryFn: () => getQuizzes({ page, pageSize }) });
}

export function useQuiz(quizId: string | null) {
  return useQuery({ queryKey: ["quiz", "detail", quizId], queryFn: () => getQuiz(quizId!), enabled: !!quizId });
}

function useInvalidateQuizzes() {
  const queryClient = useQueryClient();
  return () => {
    queryClient.invalidateQueries({ queryKey: ["quiz", "list"] });
    queryClient.invalidateQueries({ queryKey: ["quiz", "history"] });
    queryClient.invalidateQueries({ queryKey: ["quiz", "mastery"] });
    queryClient.invalidateQueries({ queryKey: ["quiz", "weak-topics"] });
  };
}

export function useGenerateQuiz() {
  const invalidate = useInvalidateQuizzes();
  return useMutation({ mutationFn: (input: GenerateQuizInput) => generateQuiz(input), onSuccess: invalidate });
}

/// Streams quiz generation instead of waiting for the full response, mirroring useStreamRecommendation/useStreamMessage.
export function useStreamGenerateQuiz() {
  const invalidate = useInvalidateQuizzes();
  const [isStreaming, setIsStreaming] = useState(false);
  const [partialText, setPartialText] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [quiz, setQuiz] = useState<QuizDetail | null>(null);
  const abortRef = useRef<AbortController | null>(null);

  const start = useCallback(
    async (input: GenerateQuizInput) => {
      setIsStreaming(true);
      setPartialText("");
      setError(null);
      setQuiz(null);

      const controller = new AbortController();
      abortRef.current = controller;

      try {
        await streamGenerateQuiz(
          input,
          (event) => {
            if (event.type === "delta") {
              setPartialText((prev) => prev + event.content);
            } else if (event.type === "complete") {
              setQuiz(event.quiz);
              invalidate();
            } else if (event.type === "error") {
              setError(event.message);
            }
          },
          controller.signal,
        );
      } catch (err) {
        if (!(err instanceof DOMException && err.name === "AbortError")) {
          setError("Couldn't generate a quiz. Make sure Ollama is running locally, then try again.");
        }
      } finally {
        setIsStreaming(false);
        abortRef.current = null;
      }
    },
    [invalidate],
  );

  const stop = useCallback(() => abortRef.current?.abort(), []);

  return { start, stop, isStreaming, partialText, error, quiz };
}

export function useSubmitQuiz() {
  const invalidate = useInvalidateQuizzes();
  return useMutation({
    mutationFn: ({ quizId, answers }: { quizId: string; answers: SubmittedAnswer[] }) => submitQuiz(quizId, answers),
    onSuccess: invalidate,
  });
}

export function useRetryQuiz() {
  return useMutation({ mutationFn: (quizId: string) => retryQuiz(quizId) });
}

export function useDeleteQuiz() {
  const invalidate = useInvalidateQuizzes();
  return useMutation({ mutationFn: (quizId: string) => deleteQuiz(quizId), onSuccess: invalidate });
}

export function useQuizHistory(page = 1, pageSize = 20) {
  return useQuery({ queryKey: ["quiz", "history", page, pageSize], queryFn: () => getQuizHistory({ page, pageSize }) });
}

export function useAttempt(attemptId: string | null) {
  return useQuery({ queryKey: ["quiz", "attempt", attemptId], queryFn: () => getAttempt(attemptId!), enabled: !!attemptId });
}

export function useTopicMastery() {
  return useQuery({ queryKey: ["quiz", "mastery"], queryFn: getTopicMastery });
}

export function useWeakTopics(take?: number) {
  return useQuery({ queryKey: ["quiz", "weak-topics", take], queryFn: () => getWeakTopics(take) });
}
