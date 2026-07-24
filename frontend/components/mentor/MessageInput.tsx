"use client";

import { useRef, useState } from "react";
import { Square, ArrowUp } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";

const MAX_LENGTH = 8000;

export function MessageInput({
  onSend,
  onStop,
  isStreaming,
  disabled,
}: {
  onSend: (content: string) => void;
  onStop: () => void;
  isStreaming: boolean;
  disabled?: boolean;
}) {
  const [value, setValue] = useState("");
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const handleSend = () => {
    const trimmed = value.trim();
    if (!trimmed || isStreaming || disabled) return;
    onSend(trimmed);
    setValue("");
    if (textareaRef.current) textareaRef.current.style.height = "auto";
  };

  const handleKeyDown = (event: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      handleSend();
    }
  };

  const handleChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    setValue(event.target.value);
    const el = event.target;
    el.style.height = "auto";
    el.style.height = `${Math.min(el.scrollHeight, 200)}px`;
  };

  return (
    <div className="flex flex-col gap-1.5 border-t border-border/70 p-3">
      <div className="glass ring-glass flex items-end gap-2 rounded-full border border-border/70 px-4 py-2 transition-shadow focus-within:ring-2 focus-within:ring-ring/40">
        <Textarea
          ref={textareaRef}
          value={value}
          onChange={handleChange}
          onKeyDown={handleKeyDown}
          placeholder="Message your Mentor... (Enter to send, Shift+Enter for a new line)"
          rows={1}
          maxLength={MAX_LENGTH}
          disabled={disabled}
          className="max-h-[200px] min-h-9 resize-none border-none bg-transparent p-0 shadow-none focus-visible:ring-0"
        />
        {isStreaming ? (
          <Button type="button" size="icon" variant="destructive" onClick={onStop} title="Stop generating">
            <Square className="size-3.5 fill-current" />
          </Button>
        ) : (
          <Button type="button" size="icon" onClick={handleSend} disabled={!value.trim() || disabled} title="Send">
            <ArrowUp />
          </Button>
        )}
      </div>
      <span className="px-1 text-right text-[0.7rem] text-muted-foreground">
        {value.length}/{MAX_LENGTH}
      </span>
    </div>
  );
}
