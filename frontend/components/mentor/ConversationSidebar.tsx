"use client";

import { useState } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { MessageSquarePlus, MoreHorizontal, Pencil, Pin, PinOff, Search, Trash2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";
import {
  useConversations,
  useCreateConversation,
  useDeleteConversation,
  useSetConversationPinned,
} from "@/lib/hooks/useMentor";
import { RenameConversationDialog } from "@/components/mentor/RenameConversationDialog";
import type { Conversation } from "@/lib/types/mentor";

function relativeTime(iso: string | null) {
  if (!iso) return "";
  const diffMs = Date.now() - new Date(iso).getTime();
  const minutes = Math.floor(diffMs / 60000);
  if (minutes < 1) return "just now";
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `${days}d ago`;
  return new Date(iso).toLocaleDateString();
}

function ConversationRow({ conversation, active, onRename }: { conversation: Conversation; active: boolean; onRename: () => void }) {
  const setPinned = useSetConversationPinned();
  const deleteConversation = useDeleteConversation();
  const router = useRouter();

  const handleDelete = () => {
    if (!window.confirm(`Delete "${conversation.title}"? This can't be undone.`)) return;
    deleteConversation.mutate(conversation.id, {
      onSuccess: () => {
        if (active) router.push("/mentor");
      },
    });
  };

  return (
    <div
      className={cn(
        "group flex items-center gap-1 rounded-xl px-2 py-1.5 text-sm transition-colors hover:bg-accent/60",
        active && "bg-gradient-brand-soft font-medium text-foreground",
      )}
    >
      <Link href={`/mentor/${conversation.id}`} className="flex-1 truncate">
        <span className="truncate">{conversation.title}</span>
        <span className="ml-1.5 text-xs text-muted-foreground">{relativeTime(conversation.lastMessageAtUtc)}</span>
      </Link>

      <DropdownMenu>
        <DropdownMenuTrigger
          render={
            <Button
              variant="ghost"
              size="icon-xs"
              className="opacity-0 group-hover:opacity-100 data-[popup-open]:opacity-100"
            />
          }
        >
          <MoreHorizontal />
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuItem onClick={() => setPinned.mutate({ conversationId: conversation.id, isPinned: !conversation.isPinned })}>
            {conversation.isPinned ? <PinOff /> : <Pin />}
            {conversation.isPinned ? "Unpin" : "Pin"}
          </DropdownMenuItem>
          <DropdownMenuItem onClick={onRename}>
            <Pencil />
            Rename
          </DropdownMenuItem>
          <DropdownMenuItem variant="destructive" onClick={handleDelete}>
            <Trash2 />
            Delete
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
}

export function ConversationSidebar() {
  const params = useParams<{ conversationId?: string }>();
  const router = useRouter();
  const [search, setSearch] = useState("");
  const [renameTarget, setRenameTarget] = useState<Conversation | null>(null);

  const { data, isLoading } = useConversations({ search: search || undefined, pageSize: 50 });
  const createConversation = useCreateConversation();

  const conversations = data?.items ?? [];
  const pinned = conversations.filter((c) => c.isPinned);
  const recent = conversations.filter((c) => !c.isPinned);

  const handleNewConversation = () => {
    createConversation.mutate(undefined, {
      onSuccess: (conversation) => router.push(`/mentor/${conversation.id}`),
    });
  };

  return (
    <div className="flex h-full w-72 shrink-0 flex-col gap-3 border-r pr-3">
      <Button className="w-full justify-start gap-2" onClick={handleNewConversation} disabled={createConversation.isPending}>
        <MessageSquarePlus />
        New chat
      </Button>

      <div className="relative">
        <Search className="absolute top-1/2 left-2.5 size-3.5 -translate-y-1/2 text-muted-foreground" />
        <Input
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Search conversations"
          className="pl-8"
        />
      </div>

      <div className="flex flex-1 flex-col gap-4 overflow-y-auto">
        {isLoading ? (
          <div className="flex flex-col gap-2">
            {Array.from({ length: 5 }).map((_, i) => (
              <Skeleton key={i} className="h-8 w-full rounded-lg" />
            ))}
          </div>
        ) : (
          <>
            {pinned.length > 0 && (
              <div className="flex flex-col gap-1">
                <span className="px-2 text-xs font-medium text-muted-foreground">Pinned</span>
                {pinned.map((conversation) => (
                  <ConversationRow
                    key={conversation.id}
                    conversation={conversation}
                    active={params.conversationId === conversation.id}
                    onRename={() => setRenameTarget(conversation)}
                  />
                ))}
              </div>
            )}

            <div className="flex flex-col gap-1">
              {recent.length > 0 && <span className="px-2 text-xs font-medium text-muted-foreground">Conversations</span>}
              {recent.map((conversation) => (
                <ConversationRow
                  key={conversation.id}
                  conversation={conversation}
                  active={params.conversationId === conversation.id}
                  onRename={() => setRenameTarget(conversation)}
                />
              ))}
              {conversations.length === 0 && (
                <p className="px-2 text-sm text-muted-foreground">{search ? "No matches." : "No conversations yet."}</p>
              )}
            </div>
          </>
        )}
      </div>

      <RenameConversationDialog conversation={renameTarget} open={!!renameTarget} onOpenChange={(open) => !open && setRenameTarget(null)} />
    </div>
  );
}
