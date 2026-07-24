"use client";

import { use } from "react";
import { useSearchParams } from "next/navigation";

import { ChatWindow } from "@/components/mentor/ChatWindow";

export default function MentorConversationPage({ params }: { params: Promise<{ conversationId: string }> }) {
  const { conversationId } = use(params);
  const searchParams = useSearchParams();
  const initialMessage = searchParams.get("ask") ?? undefined;

  return <ChatWindow conversationId={conversationId} initialMessage={initialMessage} />;
}
