"use client";

import { useAuthStore } from "@/lib/stores/authStore";

export function WelcomeWidget() {
  const user = useAuthStore((state) => state.user);
  const firstName = user?.displayName?.trim().split(/\s+/)[0];

  const today = new Date().toLocaleDateString(undefined, { weekday: "long", month: "long", day: "numeric" });

  return (
    <div className="flex flex-col gap-1">
      <h1 className="text-2xl font-semibold tracking-tight">Welcome{firstName ? `, ${firstName}` : ""} 👋</h1>
      <p className="text-muted-foreground">{today}</p>
    </div>
  );
}
