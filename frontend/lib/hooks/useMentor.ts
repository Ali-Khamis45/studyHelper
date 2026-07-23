"use client";

import { useCallback, useRef, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  createConversation,
  deleteConversation,
  getConversation,
  getConversationMessages,
  getConversations,
  renameConversation,
  setConversationPinned,
  streamMessage,
} from "@/lib/api/mentor";
import type { ConversationMessage } from "@/lib/types/mentor";

export function useConversations(params: { search?: string; pinnedOnly?: boolean; page?: number; pageSize?: number } = {}) {
  return useQuery({
    queryKey: ["mentor", "conversations", params],
    queryFn: () => getConversations(params),
  });
}

export function useConversation(conversationId: string | null) {
  return useQuery({
    queryKey: ["mentor", "conversation", conversationId],
    queryFn: () => getConversation(conversationId!),
    enabled: !!conversationId,
  });
}

export function useConversationMessages(conversationId: string | null, page = 1, pageSize = 50) {
  return useQuery({
    queryKey: ["mentor", "messages", conversationId, page, pageSize],
    queryFn: () => getConversationMessages(conversationId!, { page, pageSize }),
    enabled: !!conversationId,
  });
}

function useInvalidateConversations() {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: ["mentor", "conversations"] });
}

export function useCreateConversation() {
  const invalidate = useInvalidateConversations();
  return useMutation({ mutationFn: (title?: string) => createConversation(title), onSuccess: invalidate });
}

export function useRenameConversation() {
  const invalidate = useInvalidateConversations();
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ conversationId, title }: { conversationId: string; title: string }) => renameConversation(conversationId, title),
    onSuccess: (conversation) => {
      queryClient.setQueryData(["mentor", "conversation", conversation.id], conversation);
      invalidate();
    },
  });
}

export function useSetConversationPinned() {
  const invalidate = useInvalidateConversations();
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ conversationId, isPinned }: { conversationId: string; isPinned: boolean }) => setConversationPinned(conversationId, isPinned),
    onSuccess: (conversation) => {
      queryClient.setQueryData(["mentor", "conversation", conversation.id], conversation);
      invalidate();
    },
  });
}

export function useDeleteConversation() {
  const invalidate = useInvalidateConversations();
  return useMutation({ mutationFn: (conversationId: string) => deleteConversation(conversationId), onSuccess: invalidate });
}

/// Drives the streaming send-message flow: appends the user's message optimistically, accumulates
/// delta text token-by-token, then swaps the placeholder for the persisted assistant message once
/// the AiKernel -> Provider -> Persistence pipeline finishes. Mirrors useStreamRecommendation's shape.
export function useStreamMessage(conversationId: string | null) {
  const queryClient = useQueryClient();
  const [isStreaming, setIsStreaming] = useState(false);
  const [partialText, setPartialText] = useState("");
  const [error, setError] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);

  const invalidateConversations = useInvalidateConversations();

  const send = useCallback(
    async (content: string) => {
      if (!conversationId) return;

      setIsStreaming(true);
      setPartialText("");
      setError(null);

      const controller = new AbortController();
      abortRef.current = controller;

      const optimisticUserMessage: ConversationMessage = {
        id: `optimistic-${Date.now()}`,
        conversationId,
        role: "User",
        content,
        agentType: null,
        modelUsed: null,
        promptTokens: null,
        completionTokens: null,
        createdAtUtc: new Date().toISOString(),
      };

      queryClient.setQueryData(["mentor", "messages", conversationId, 1, 50], (prev: { items: ConversationMessage[] } | undefined) =>
        prev ? { ...prev, items: [...prev.items, optimisticUserMessage] } : prev,
      );

      try {
        await streamMessage(
          conversationId,
          content,
          (event) => {
            if (event.type === "delta") {
              setPartialText((prev) => prev + event.content);
            } else if (event.type === "complete") {
              queryClient.invalidateQueries({ queryKey: ["mentor", "messages", conversationId] });
              queryClient.invalidateQueries({ queryKey: ["mentor", "conversation", conversationId] });
              invalidateConversations();
            } else if (event.type === "error") {
              setError(event.message);
            }
          },
          controller.signal,
        );
      } catch (err) {
        if (!(err instanceof DOMException && err.name === "AbortError")) {
          setError("Couldn't reach the Mentor. Make sure Ollama is running locally, then try again.");
        }
        // On cancellation the user's message was already persisted server-side before generation
        // started — refresh so the list reflects real state instead of the optimistic placeholder.
        queryClient.invalidateQueries({ queryKey: ["mentor", "messages", conversationId] });
      } finally {
        setIsStreaming(false);
        setPartialText("");
        abortRef.current = null;
      }
    },
    [conversationId, queryClient, invalidateConversations],
  );

  const stop = useCallback(() => {
    abortRef.current?.abort();
  }, []);

  return { send, stop, isStreaming, partialText, error };
}
