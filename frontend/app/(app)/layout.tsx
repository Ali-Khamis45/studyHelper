"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { logout as logoutRequest, refresh as refreshRequest } from "@/lib/api/auth";
import { useAuthStore } from "@/lib/stores/authStore";

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
      <div className="flex min-h-screen flex-col gap-4 p-8">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-32 w-full" />
      </div>
    );
  }

  return (
    <div className="flex min-h-screen flex-col">
      <header className="flex items-center justify-between border-b px-6 py-4">
        <span className="font-semibold">AI Study OS</span>
        <div className="flex items-center gap-4">
          {user && <span className="text-sm text-muted-foreground">{user.email}</span>}
          <Button variant="outline" size="sm" onClick={handleLogout}>
            Sign out
          </Button>
        </div>
      </header>
      <main className="flex-1 p-6">{children}</main>
    </div>
  );
}
