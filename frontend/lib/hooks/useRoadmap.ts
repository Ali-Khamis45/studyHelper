"use client";

import { useCallback, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  completeTopic,
  deleteRoadmap,
  generateRoadmap,
  getRoadmap,
  getRoadmaps,
  streamGenerateRoadmap,
} from "@/lib/api/roadmap";
import type { GenerateRoadmapInput, Roadmap } from "@/lib/types/roadmap";

export function useRoadmaps() {
  return useQuery({ queryKey: ["roadmap", "list"], queryFn: getRoadmaps });
}

export function useRoadmapDetail(roadmapId: string | null) {
  return useQuery({ queryKey: ["roadmap", "detail", roadmapId], queryFn: () => getRoadmap(roadmapId!), enabled: !!roadmapId });
}

function useInvalidateRoadmaps() {
  const queryClient = useQueryClient();
  return (roadmapId?: string) => {
    queryClient.invalidateQueries({ queryKey: ["roadmap", "list"] });
    if (roadmapId) queryClient.invalidateQueries({ queryKey: ["roadmap", "detail", roadmapId] });
  };
}

export function useGenerateRoadmap() {
  const invalidate = useInvalidateRoadmaps();
  return useMutation({ mutationFn: (input: GenerateRoadmapInput) => generateRoadmap(input), onSuccess: () => invalidate() });
}

/// Streams roadmap generation instead of waiting for the full response, mirroring useStreamGenerateQuiz/useStreamRecommendation.
export function useStreamGenerateRoadmap() {
  const invalidate = useInvalidateRoadmaps();
  const [isStreaming, setIsStreaming] = useState(false);
  const [partialText, setPartialText] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [roadmap, setRoadmap] = useState<Roadmap | null>(null);

  const start = useCallback(
    async (input: GenerateRoadmapInput) => {
      setIsStreaming(true);
      setPartialText("");
      setError(null);
      setRoadmap(null);

      try {
        await streamGenerateRoadmap(input, (event) => {
          if (event.type === "delta") {
            setPartialText((prev) => prev + event.content);
          } else if (event.type === "complete") {
            setRoadmap(event.roadmap);
            invalidate();
          } else if (event.type === "error") {
            setError(event.message);
          }
        });
      } catch {
        setError("Couldn't generate a roadmap. Make sure Ollama is running locally, then try again.");
      } finally {
        setIsStreaming(false);
      }
    },
    [invalidate],
  );

  return { start, isStreaming, partialText, error, roadmap };
}

export function useCompleteTopic(roadmapId: string) {
  const invalidate = useInvalidateRoadmaps();
  return useMutation({
    mutationFn: ({ topicId, completed }: { topicId: string; completed: boolean }) => completeTopic(roadmapId, topicId, completed),
    onSuccess: () => invalidate(roadmapId),
  });
}

export function useDeleteRoadmap() {
  const invalidate = useInvalidateRoadmaps();
  return useMutation({ mutationFn: (roadmapId: string) => deleteRoadmap(roadmapId), onSuccess: () => invalidate() });
}
