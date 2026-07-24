import { CheckCircle2, CircleDot, Loader2, Lock, Sparkles } from "lucide-react";

import { cn } from "@/lib/utils";
import type { TopicStatusValue } from "@/lib/types/roadmap";

const STATUS_META: Record<TopicStatusValue, { label: string; icon: typeof Lock; className: string }> = {
  Locked: { label: "Locked", icon: Lock, className: "bg-muted text-muted-foreground" },
  Available: { label: "Available", icon: CircleDot, className: "bg-warning/15 text-warning" },
  InProgress: { label: "In Progress", icon: Loader2, className: "bg-brand-cyan/15 text-brand-cyan" },
  Completed: { label: "Completed", icon: CheckCircle2, className: "bg-success/15 text-success" },
  Mastered: { label: "Mastered", icon: Sparkles, className: "bg-gradient-brand text-primary-foreground" },
};

export function TopicStatusBadge({ status, className }: { status: TopicStatusValue; className?: string }) {
  const meta = STATUS_META[status];
  const Icon = meta.icon;

  return (
    <span className={cn("inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium", meta.className, className)}>
      <Icon className={cn("size-3", status === "InProgress" && "animate-spin")} />
      {meta.label}
    </span>
  );
}
