"use client";

import { useRouter } from "next/navigation";
import { Bot } from "lucide-react";

import { Button } from "@/components/ui/button";
import { useCreateConversation } from "@/lib/hooks/useMentor";

export default function MentorPage() {
  const router = useRouter();
  const createConversation = useCreateConversation();

  const handleNewConversation = () => {
    createConversation.mutate(undefined, {
      onSuccess: (conversation) => router.push(`/mentor/${conversation.id}`),
    });
  };

  return (
    <div className="flex h-full flex-col items-center justify-center gap-3 rounded-xl border bg-background text-center">
      <Bot className="size-10 text-muted-foreground" />
      <div className="flex flex-col gap-1">
        <h2 className="text-lg font-semibold">AI Mentor</h2>
        <p className="max-w-sm text-sm text-muted-foreground">
          Select a conversation, or start a new one — your Mentor knows your goals, plan, and progress.
        </p>
      </div>
      <Button onClick={handleNewConversation} disabled={createConversation.isPending}>
        Start a new conversation
      </Button>
    </div>
  );
}
