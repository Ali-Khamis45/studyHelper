"use client";

import { Flame, Sparkles } from "lucide-react";

import { useAuthStore } from "@/lib/stores/authStore";
import { useTodayPlan } from "@/lib/hooks/usePlanner";

function timeOfDayGreeting(hour: number) {
  if (hour < 5) return { label: "Good Night", emoji: "🌙" };
  if (hour < 12) return { label: "Good Morning", emoji: "👋" };
  if (hour < 18) return { label: "Good Afternoon", emoji: "☀️" };
  return { label: "Good Evening", emoji: "🌙" };
}

export function WelcomeWidget() {
  const user = useAuthStore((state) => state.user);
  const firstName = user?.displayName?.trim().split(/\s+/)[0];
  const { data: plan } = useTodayPlan();

  const today = new Date().toLocaleDateString(undefined, { weekday: "long", month: "long", day: "numeric" });
  const greeting = timeOfDayGreeting(new Date().getHours());

  return (
    <div className="ring-glass relative overflow-hidden rounded-2xl bg-gradient-brand p-6 text-primary-foreground sm:p-8">
      <div
        className="pointer-events-none absolute inset-0 opacity-60"
        style={{
          backgroundImage:
            "radial-gradient(circle at 85% 0%, color-mix(in oklch, var(--brand-cyan) 45%, transparent) 0%, transparent 55%)",
        }}
      />
      <div className="relative flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div className="flex flex-col gap-1.5">
          <span className="flex w-fit items-center gap-1.5 rounded-full bg-primary-foreground/15 px-2.5 py-1 text-xs font-medium backdrop-blur">
            <Sparkles className="size-3" />
            {today}
          </span>
          <h1 className="text-3xl font-bold tracking-tight">
            {greeting.emoji} {greeting.label}{firstName ? `, ${firstName}` : ""}
          </h1>
          <p className="text-primary-foreground/75">Ready to continue your learning journey?</p>
        </div>

        {plan && plan.studyStreak > 0 && (
          <div className="flex items-center gap-2 rounded-2xl bg-primary-foreground/10 px-4 py-2.5 backdrop-blur">
            <Flame className="size-5 text-orange-300" />
            <div>
              <p className="text-lg leading-none font-bold tabular-nums">{plan.studyStreak}</p>
              <p className="text-xs text-primary-foreground/70">day streak</p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
