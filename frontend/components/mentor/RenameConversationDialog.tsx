"use client";

import { useState } from "react";

import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { useRenameConversation } from "@/lib/hooks/useMentor";
import type { Conversation } from "@/lib/types/mentor";

// Keyed by conversation.id in the parent below, so React remounts this fresh (and re-derives
// `title`'s initial value from the new conversation) each time the rename target changes — no
// effect needed to "sync" state from a prop, which the React Compiler's lint rule flags as
// cascading-render-prone.
function RenameForm({ conversation, onOpenChange }: { conversation: Conversation; onOpenChange: (open: boolean) => void }) {
  const [title, setTitle] = useState(conversation.title);
  const renameConversation = useRenameConversation();

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (!title.trim()) return;

    renameConversation.mutate(
      { conversationId: conversation.id, title: title.trim() },
      { onSuccess: () => onOpenChange(false) },
    );
  };

  return (
    <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
      <Input value={title} onChange={(event) => setTitle(event.target.value)} maxLength={200} autoFocus />
      <DialogFooter>
        <Button type="submit" disabled={!title.trim() || renameConversation.isPending}>
          {renameConversation.isPending ? "Saving…" : "Save"}
        </Button>
      </DialogFooter>
    </form>
  );
}

export function RenameConversationDialog({
  conversation,
  open,
  onOpenChange,
}: {
  conversation: Conversation | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Rename conversation</DialogTitle>
        </DialogHeader>
        {conversation && <RenameForm key={conversation.id} conversation={conversation} onOpenChange={onOpenChange} />}
      </DialogContent>
    </Dialog>
  );
}
