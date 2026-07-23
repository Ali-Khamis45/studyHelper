"use client";

import Link from "next/link";
import { MessageCircle } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { buttonVariants } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useConversations } from "@/lib/hooks/useMentor";

export function RecentConversationWidget() {
  const { data, isLoading } = useConversations({ page: 1, pageSize: 1 });

  if (isLoading) return <Skeleton className="h-32 rounded-xl" />;

  const conversation = data?.items[0];

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base">
          <MessageCircle className="size-4" />
          Mentor
        </CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-3">
        {!conversation ? (
          <p className="text-sm text-muted-foreground">No conversations yet — ask your Mentor anything.</p>
        ) : (
          <div className="flex flex-col gap-1">
            <p className="truncate text-sm font-medium">{conversation.title}</p>
            <p className="text-xs text-muted-foreground">
              {conversation.messageCount} message{conversation.messageCount === 1 ? "" : "s"}
            </p>
          </div>
        )}
        <Link
          href={conversation ? `/mentor/${conversation.id}` : "/mentor"}
          className={buttonVariants({ variant: "outline", size: "sm", className: "w-fit" })}
        >
          {conversation ? "Continue chat" : "Start a conversation"}
        </Link>
      </CardContent>
    </Card>
  );
}
