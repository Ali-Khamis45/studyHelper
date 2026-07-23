"use client";

import { useState } from "react";
import { Check, Copy, RotateCcw } from "lucide-react";
import ReactMarkdown from "react-markdown";
import rehypeHighlight from "rehype-highlight";
import remarkGfm from "remark-gfm";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import type { ConversationMessage } from "@/lib/types/mentor";

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

  const handleCopy = async () => {
    await navigator.clipboard.writeText(message.content);
    setCopied(true);
    setTimeout(() => setCopied(false), 1500);
  };

  return (
    <div className={cn("flex flex-col gap-1", isUser ? "items-end" : "items-start")}>
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
          "max-w-[85%] rounded-2xl px-4 py-2.5 text-sm leading-relaxed",
          isUser ? "bg-primary text-primary-foreground" : "bg-muted text-foreground",
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
  );
}
