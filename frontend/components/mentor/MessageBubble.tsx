"use client";

import { useState } from "react";
import { Bot, Check, Copy, RotateCcw } from "lucide-react";
import ReactMarkdown from "react-markdown";
import rehypeHighlight from "rehype-highlight";
import remarkGfm from "remark-gfm";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { useAuthStore } from "@/lib/stores/authStore";
import type { ConversationMessage } from "@/lib/types/mentor";

function initials(name: string) {
  const parts = name.trim().split(/\s+/);
  return ((parts[0]?.[0] ?? "") + (parts[1]?.[0] ?? "")).toUpperCase() || "?";
}

function formatTime(iso: string) {
  return new Date(iso).toLocaleTimeString([], { hour: "numeric", minute: "2-digit" });
}

export function MessageBubble({
  message,
  onRegenerate,
  canRegenerate,
}: {
  message: ConversationMessage;
  onRegenerate?: () => void;
  canRegenerate?: boolean;
}) {
  const [copied, setCopied] = useState(false);
  const isUser = message.role === "User";
  const user = useAuthStore((state) => state.user);

  const handleCopy = async () => {
    await navigator.clipboard.writeText(message.content);
    setCopied(true);
    setTimeout(() => setCopied(false), 1500);
  };

  return (
    <div className={cn("flex items-end gap-2.5", isUser ? "flex-row-reverse" : "flex-row")}>
      <span
        className={cn(
          "mb-1 flex size-7 shrink-0 items-center justify-center rounded-full text-[0.65rem] font-semibold text-primary-foreground",
          isUser ? "bg-gradient-brand" : "bg-gradient-to-br from-brand-cyan to-primary",
        )}
      >
        {isUser ? initials(user?.displayName || user?.email || "You") : <Bot className="size-3.5" />}
      </span>

      <div className={cn("flex max-w-[85%] flex-col gap-1", isUser ? "items-end" : "items-start")}>
        <div className={cn("flex items-center gap-2 px-1 text-xs text-muted-foreground", isUser && "flex-row-reverse")}>
          <span>{isUser ? "You" : "Mentor"}</span>
          {message.agentType && (
            <Badge variant="secondary" className="h-4 px-1.5 text-[0.65rem]">
              {message.agentType}
            </Badge>
          )}
          <span>{formatTime(message.createdAtUtc)}</span>
        </div>

        <div
          className={cn(
            "rounded-2xl px-4 py-2.5 text-sm leading-relaxed shadow-sm",
            isUser
              ? "rounded-br-md bg-gradient-brand text-primary-foreground"
              : "glass ring-glass rounded-bl-md text-foreground",
          )}
        >
          {isUser ? (
            <p className="whitespace-pre-wrap">{message.content}</p>
          ) : (
            <div className="prose prose-sm dark:prose-invert max-w-none prose-pre:bg-background/60 prose-pre:text-foreground">
              <ReactMarkdown remarkPlugins={[remarkGfm]} rehypePlugins={[rehypeHighlight]}>
                {message.content}
              </ReactMarkdown>
            </div>
          )}
        </div>

        {!isUser && (
          <div className="flex items-center gap-1 px-1">
            <Button variant="ghost" size="icon-xs" onClick={handleCopy} title="Copy">
              {copied ? <Check className="text-primary" /> : <Copy />}
            </Button>
            {canRegenerate && (
              <Button variant="ghost" size="icon-xs" onClick={onRegenerate} title="Regenerate — asks again as a new message">
                <RotateCcw />
              </Button>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
