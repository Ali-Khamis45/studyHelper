"use client";

import { use } from "react";

import { ChatWindow } from "@/components/mentor/ChatWindow";

export default function MentorConversationPage({ params }: { params: Promise<{ conversationId: string }> }) {
  const { conversationId } = use(params);

  return <ChatWindow conversationId={conversationId} />;
}
