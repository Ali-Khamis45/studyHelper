"use client";

import { useEffect, useRef } from "react";
import { Bot } from "lucide-react";

import { Skeleton } from "@/components/ui/skeleton";
import { useConversation, useConversationMessages, useStreamMessage } from "@/lib/hooks/useMentor";
import { MessageBubble } from "@/components/mentor/MessageBubble";
import { MessageInput } from "@/components/mentor/MessageInput";
import { TypingIndicator } from "@/components/mentor/TypingIndicator";
import type { ConversationMessage } from "@/lib/types/mentor";

export function ChatWindow({ conversationId }: { conversationId: string }) {
  const { data: conversation } = useConversation(conversationId);
  const { data: messagesPage, isLoading } = useConversationMessages(conversationId);
  const { send, stop, isStreaming, partialText, error } = useStreamMessage(conversationId);

  const bottomRef = useRef<HTMLDivElement>(null);

  const messages = messagesPage?.items ?? [];

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages.length, partialText]);

  const lastUserMessage = [...messages].reverse().find((m) => m.role === "User");
  const lastMessage = messages[messages.length - 1];
  const canRegenerate = !isStreaming && !!lastUserMessage && lastMessage?.role === "Assistant";

  return (
    <div className="flex h-full flex-col overflow-hidden rounded-xl border bg-background">
      <div className="flex items-center justify-between border-b px-4 py-3">
        <div className="flex flex-col">
          <span className="text-sm font-semibold">{conversation?.title ?? "Conversation"}</span>
          {conversation && (
            <span className="text-xs text-muted-foreground">
              {conversation.messageCount} message{conversation.messageCount === 1 ? "" : "s"}
            </span>
          )}
        </div>
      </div>

      <div className="flex-1 overflow-y-auto px-4 py-4">
        {isLoading ? (
          <div className="flex flex-col gap-4">
            <Skeleton className="h-16 w-2/3 rounded-2xl" />
            <Skeleton className="ml-auto h-10 w-1/2 rounded-2xl" />
            <Skeleton className="h-20 w-3/4 rounded-2xl" />
          </div>
        ) : messages.length === 0 && !isStreaming ? (
          <div className="flex h-full flex-col items-center justify-center gap-2 text-center text-muted-foreground">
            <Bot className="size-8" />
            <p className="text-sm">Ask about your goals, today&apos;s plan, a concept you&apos;re stuck on, or anything else.</p>
          </div>
        ) : (
          <div className="flex flex-col gap-4">
            {messages.map((message: ConversationMessage) => (
              <MessageBubble
                key={message.id}
                message={message}
                canRegenerate={canRegenerate && message.id === lastMessage.id}
                onRegenerate={() => lastUserMessage && send(lastUserMessage.content)}
              />
            ))}

            {isStreaming && partialText.length === 0 && <TypingIndicator />}

            {isStreaming && partialText.length > 0 && (
              <MessageBubble
                message={{
                  id: "streaming",
                  conversationId,
                  role: "Assistant",
                  content: partialText,
                  agentType: null,
                  modelUsed: null,
                  promptTokens: null,
                  completionTokens: null,
                  createdAtUtc: new Date().toISOString(),
                }}
              />
            )}
          </div>
        )}

        {error && <p className="mt-3 rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{error}</p>}

        <div ref={bottomRef} />
      </div>

      <MessageInput onSend={send} onStop={stop} isStreaming={isStreaming} />
    </div>
  );
}
