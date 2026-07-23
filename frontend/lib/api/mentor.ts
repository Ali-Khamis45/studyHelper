import { API_BASE_URL, ApiError, apiFetch } from "@/lib/api/client";
import { useAuthStore } from "@/lib/stores/authStore";
import type { Conversation, ConversationMessage, MentorStreamEvent, PagedResult } from "@/lib/types/mentor";

export function getConversations(params: { search?: string; pinnedOnly?: boolean; page?: number; pageSize?: number } = {}) {
  const query = new URLSearchParams();
  if (params.search) query.set("search", params.search);
  if (params.pinnedOnly) query.set("pinned", "true");
  if (params.page) query.set("page", String(params.page));
  if (params.pageSize) query.set("pageSize", String(params.pageSize));

  const qs = query.toString();
  return apiFetch<PagedResult<Conversation>>(`/api/v1/mentor/conversations${qs ? `?${qs}` : ""}`);
}

export function createConversation(title?: string) {
  return apiFetch<Conversation>("/api/v1/mentor/conversations", { method: "POST", body: { title: title ?? null } });
}

export function getConversation(conversationId: string) {
  return apiFetch<Conversation>(`/api/v1/mentor/conversations/${conversationId}`);
}

export function renameConversation(conversationId: string, title: string) {
  return apiFetch<Conversation>(`/api/v1/mentor/conversations/${conversationId}/rename`, { method: "PATCH", body: { title } });
}

export function setConversationPinned(conversationId: string, isPinned: boolean) {
  return apiFetch<Conversation>(`/api/v1/mentor/conversations/${conversationId}/pin`, { method: "PATCH", body: { isPinned } });
}

export function deleteConversation(conversationId: string) {
  return apiFetch<void>(`/api/v1/mentor/conversations/${conversationId}`, { method: "DELETE" });
}

export function getConversationMessages(conversationId: string, params: { page?: number; pageSize?: number } = {}) {
  const query = new URLSearchParams();
  if (params.page) query.set("page", String(params.page));
  if (params.pageSize) query.set("pageSize", String(params.pageSize));

  const qs = query.toString();
  return apiFetch<PagedResult<ConversationMessage>>(`/api/v1/mentor/conversations/${conversationId}/messages${qs ? `?${qs}` : ""}`);
}

export function sendMessage(conversationId: string, content: string) {
  return apiFetch<ConversationMessage>(`/api/v1/mentor/conversations/${conversationId}/messages`, { method: "POST", body: { content } });
}

// NDJSON, not apiFetch — see lib/api/planner.ts's streamRecommendation for why (manual reader,
// Authorization header EventSource can't send). Mirrors that implementation exactly.
export async function streamMessage(conversationId: string, content: string, onEvent: (event: MentorStreamEvent) => void, signal?: AbortSignal) {
  const accessToken = useAuthStore.getState().accessToken;

  const res = await fetch(`${API_BASE_URL}/api/v1/mentor/conversations/${conversationId}/messages/stream`, {
    method: "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
      ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
    },
    body: JSON.stringify({ content }),
    signal,
  });

  if (!res.ok || !res.body) {
    const errorBody = await res.json().catch(() => null);
    throw new ApiError(res.status, errorBody);
  }

  const reader = res.body.getReader();
  const decoder = new TextDecoder();
  let buffer = "";

  while (true) {
    const { done, value } = await reader.read();
    if (done) break;

    buffer += decoder.decode(value, { stream: true });

    let newlineIndex: number;
    while ((newlineIndex = buffer.indexOf("\n")) >= 0) {
      const line = buffer.slice(0, newlineIndex).trim();
      buffer = buffer.slice(newlineIndex + 1);
      if (line) onEvent(JSON.parse(line) as MentorStreamEvent);
    }
  }
}
