"use client";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useAuthStore } from "@/lib/stores/authStore";

export default function DashboardPage() {
  const user = useAuthStore((state) => state.user);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Welcome{user ? `, ${user.displayName}` : ""}.</CardTitle>
        <CardDescription>
          The full dashboard (goals, planner, analytics) lands in M10 — this page exists in M1 to
          prove the protected-route flow end-to-end.
        </CardDescription>
      </CardHeader>
      <CardContent />
    </Card>
  );
}
