"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";

import { MobileNav } from "@/components/layout/MobileNav";
import { Sidebar } from "@/components/layout/Sidebar";
import { Topbar } from "@/components/layout/Topbar";
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
      <div className="flex min-h-screen flex-col">
        <div className="flex h-16 items-center justify-between border-b border-border/70 px-6">
          <Skeleton className="h-6 w-32" />
          <Skeleton className="size-8 rounded-full" />
        </div>
        <div className="flex-1 p-6">
          <Skeleton className="h-40 w-full max-w-2xl rounded-2xl" />
        </div>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen">
      <Sidebar />
      <div className="flex min-w-0 flex-1 flex-col">
        <Topbar user={user} onLogout={handleLogout} />
        <main className="flex-1 p-4 md:p-8">{children}</main>
        <MobileNav />
      </div>
    </div>
  );
}
