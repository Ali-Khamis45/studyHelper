"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";

import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { logout as logoutRequest, refresh as refreshRequest } from "@/lib/api/auth";
import { useAuthStore } from "@/lib/stores/authStore";

function initials(name: string) {
  const parts = name.trim().split(/\s+/);
  return ((parts[0]?.[0] ?? "") + (parts[1]?.[0] ?? "")).toUpperCase() || "?";
}

export default function AppLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const { user, isAuthenticated, setSession, clearSession } = useAuthStore();
  const [isBootstrapping, setIsBootstrapping] = useState(!isAuthenticated);

  useEffect(() => {
    // isBootstrapping is only ever true on mount when there was no in-memory access token yet
    // (see useState initializer below), so this effect has nothing to do in the already-authenticated
    // case — it only needs to run the silent-refresh bootstrap once.
    if (!isBootstrapping) return;

    // A hard refresh loses the in-memory access token; the httpOnly refresh cookie survives it,
    // so silently exchange it for a fresh access token before deciding the user is logged out.
    refreshRequest()
      .then((data) => setSession(data.user, data.accessToken))
      .catch(() => router.replace("/login"))
      .finally(() => setIsBootstrapping(false));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleLogout = async () => {
    await logoutRequest().catch(() => null);
    clearSession();
    router.replace("/login");
  };

  if (isBootstrapping) {
    return (
      <div className="flex min-h-screen flex-col">
        <div className="flex h-16 items-center justify-between border-b px-6">
          <Skeleton className="h-6 w-32" />
          <Skeleton className="size-8 rounded-full" />
        </div>
        <div className="flex-1 p-6">
          <Skeleton className="h-40 w-full max-w-2xl rounded-xl" />
        </div>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen flex-col bg-muted/30">
      <header className="sticky top-0 z-10 flex h-16 items-center justify-between border-b bg-background/80 px-6 backdrop-blur">
        <div className="flex items-center gap-2 text-sm font-semibold tracking-tight">
          <span className="flex size-7 items-center justify-center rounded-lg bg-primary text-xs font-bold text-primary-foreground">
            AI
          </span>
          Study OS
        </div>
        <div className="flex items-center gap-3">
          {user && (
            <div className="flex items-center gap-2">
              <Avatar className="size-8">
                <AvatarFallback className="bg-accent text-xs text-accent-foreground">
                  {initials(user.displayName || user.email)}
                </AvatarFallback>
              </Avatar>
              <span className="hidden text-sm text-muted-foreground sm:inline">{user.email}</span>
            </div>
          )}
          <Button variant="outline" size="sm" onClick={handleLogout}>
            Sign out
          </Button>
        </div>
      </header>
      <main className="flex-1 p-6 md:p-8">{children}</main>
    </div>
  );
}
