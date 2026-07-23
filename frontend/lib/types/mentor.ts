export type MessageRole = "User" | "Assistant";

export interface Conversation {
  id: string;
  title: string;
  isPinned: boolean;
  messageCount: number;
  totalPromptTokens: number;
  totalCompletionTokens: number;
  createdAtUtc: string;
  updatedAtUtc: string;
  lastMessageAtUtc: string | null;
}

export interface ConversationMessage {
  id: string;
  conversationId: string;
  role: MessageRole;
  content: string;
  agentType: string | null;
  modelUsed: string | null;
  promptTokens: number | null;
  completionTokens: number | null;
  createdAtUtc: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasMore: boolean;
}

export type MentorStreamEvent =
  | { type: "delta"; content: string }
  | { type: "complete"; message: ConversationMessage }
  | { type: "error"; message: string };
