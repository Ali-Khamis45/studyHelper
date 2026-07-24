"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { ChevronRight, Circle, ExternalLink, MessageCircle, Sparkles, SquareCheckBig } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { useCreateConversation } from "@/lib/hooks/useMentor";
import { useGenerateQuiz } from "@/lib/hooks/useQuiz";
import { useCompleteTopic } from "@/lib/hooks/useRoadmap";
import type { RoadmapTopicNode } from "@/lib/types/roadmap";
import type { DifficultyValue } from "@/lib/types/quiz";
import { cn } from "@/lib/utils";
import { TopicStatusBadge } from "@/components/roadmap/TopicStatusBadge";

const DIFFICULTY_TO_QUIZ: Record<RoadmapTopicNode["difficulty"], DifficultyValue> = {
  Beginner: "Easy",
  Intermediate: "Medium",
  Advanced: "Hard",
};

export function TopicNode({ topic, roadmapId, depth }: { topic: RoadmapTopicNode; roadmapId: string; depth: number }) {
  const [expanded, setExpanded] = useState(depth === 0);
  const hasChildren = topic.children.length > 0;
  const locked = topic.status === "Locked";

  return (
    <div className="flex flex-col gap-2" style={depth > 0 ? { marginLeft: 20 } : undefined}>
      <div
        className={cn(
          "glass ring-glass flex items-start gap-2 rounded-xl border border-border p-3 transition-opacity",
          locked && "opacity-70",
        )}
      >
        <button
          type="button"
          onClick={() => setExpanded((e) => !e)}
          className="mt-0.5 flex size-5 shrink-0 items-center justify-center rounded-md text-muted-foreground hover:bg-muted"
          aria-label={expanded ? "Collapse" : "Expand"}
        >
          <ChevronRight className={cn("size-4 transition-transform", expanded && "rotate-90")} />
        </button>

        <div className="min-w-0 flex-1">
          <div className="flex flex-wrap items-center gap-2">
            <span className={cn("font-medium", hasChildren ? "text-sm" : "text-sm")}>{topic.title}</span>
            <TopicStatusBadge status={topic.status} />
            {!hasChildren && <Badge variant="outline">{topic.difficulty}</Badge>}
            {!hasChildren && topic.estimatedHours > 0 && (
              <span className="text-xs text-muted-foreground">{topic.estimatedHours}h</span>
            )}
          </div>
          {topic.description && <p className="mt-1 text-sm text-muted-foreground">{topic.description}</p>}
        </div>
      </div>

      {expanded && (
        <div className="flex flex-col gap-2">
          {!hasChildren && <TopicDetail topic={topic} roadmapId={roadmapId} locked={locked} />}
          {hasChildren &&
            topic.children.map((child) => <TopicNode key={child.id} topic={child} roadmapId={roadmapId} depth={depth + 1} />)}
        </div>
      )}
    </div>
  );
}

function TopicDetail({ topic, roadmapId, locked }: { topic: RoadmapTopicNode; roadmapId: string; locked: boolean }) {
  const router = useRouter();
  const createConversation = useCreateConversation();
  const generateQuiz = useGenerateQuiz();
  const completeTopic = useCompleteTopic(roadmapId);

  const unmetPrerequisites = topic.prerequisites.filter((p) => !p.met);

  const handleAskMentor = () => {
    createConversation.mutate(topic.title, {
      onSuccess: (conversation) => {
        const seed = `Explain "${topic.title}" to me. ${topic.description}`.trim();
        router.push(`/mentor/${conversation.id}?ask=${encodeURIComponent(seed)}`);
      },
    });
  };

  const handleTakeQuiz = () => {
    generateQuiz.mutate(
      {
        topic: topic.title,
        goalId: null,
        difficulty: DIFFICULTY_TO_QUIZ[topic.difficulty],
        questionTypes: ["MultipleChoice", "TrueFalse"],
        questionCount: 5,
        quizType: "Standard",
      },
      { onSuccess: (quiz) => router.push(`/quiz/${quiz.id}`) },
    );
  };

  return (
    <div className="ml-7 flex flex-col gap-3 rounded-xl border border-dashed border-border p-3">
      {topic.prerequisites.length > 0 && (
        <div className="flex flex-col gap-1.5">
          <span className="text-xs font-semibold tracking-wide text-muted-foreground uppercase">
            Before learning {topic.title}, complete
          </span>
          <div className="flex flex-wrap gap-1.5">
            {topic.prerequisites.map((p) => (
              <span
                key={p.topicId}
                className={cn(
                  "inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium",
                  p.met ? "bg-success/15 text-success" : "bg-destructive/10 text-destructive",
                )}
              >
                {p.met ? "✔" : "✘"} {p.title}
              </span>
            ))}
          </div>
        </div>
      )}

      {topic.resources.length > 0 && (
        <div className="flex flex-col gap-1.5">
          <span className="text-xs font-semibold tracking-wide text-muted-foreground uppercase">Resources</span>
          <div className="flex flex-col gap-1">
            {topic.resources.map((r, i) =>
              r.url ? (
                <a key={i} href={r.url} target="_blank" rel="noreferrer" className="flex items-center gap-1.5 text-sm text-primary hover:underline">
                  <ExternalLink className="size-3" />
                  {r.title}
                  <Badge variant="outline" className="ml-1">
                    {r.type}
                  </Badge>
                </a>
              ) : (
                <span key={i} className="flex items-center gap-1.5 text-sm text-muted-foreground">
                  <Circle className="size-2 fill-current" />
                  {r.title}
                  <Badge variant="outline" className="ml-1">
                    {r.type}
                  </Badge>
                </span>
              ),
            )}
          </div>
        </div>
      )}

      {topic.suggestedProjects.length > 0 && (
        <div className="flex flex-col gap-1.5">
          <span className="text-xs font-semibold tracking-wide text-muted-foreground uppercase">Suggested projects</span>
          <div className="flex flex-wrap gap-1.5">
            {topic.suggestedProjects.map((p, i) => (
              <Badge key={i} variant="secondary">
                {p}
              </Badge>
            ))}
          </div>
        </div>
      )}

      {topic.masteryScore > 0 && (
        <span className="text-xs text-muted-foreground">Mastery: {Math.round(topic.masteryScore * 100)}%</span>
      )}

      <div className="flex flex-wrap items-center gap-2 pt-1">
        <Button size="sm" variant="outline" onClick={handleAskMentor} disabled={createConversation.isPending}>
          <MessageCircle /> Ask Mentor
        </Button>
        <Button size="sm" variant="outline" onClick={handleTakeQuiz} disabled={locked || generateQuiz.isPending}>
          <Sparkles /> {generateQuiz.isPending ? "Generating…" : "Take Quiz"}
        </Button>
        <Button
          size="sm"
          variant={topic.manuallyCompleted ? "secondary" : "default"}
          disabled={locked || completeTopic.isPending}
          onClick={() => completeTopic.mutate({ topicId: topic.id, completed: !topic.manuallyCompleted })}
        >
          <SquareCheckBig /> {topic.manuallyCompleted ? "Completed" : "Mark Complete"}
        </Button>
      </div>

      {locked && unmetPrerequisites.length > 0 && (
        <p className="text-xs text-warning">Complete the prerequisites above to unlock this topic.</p>
      )}
    </div>
  );
}
