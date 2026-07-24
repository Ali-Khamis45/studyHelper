"use client";

import Link from "next/link";
import { Bell, LogOut, Search } from "lucide-react";

import { AiStatusBadge } from "@/components/planner/AiStatusBadge";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import { ThemeSwitcher } from "@/components/layout/ThemeSwitcher";
import type { AuthUser } from "@/lib/stores/authStore";

function initials(name: string) {
  const parts = name.trim().split(/\s+/);
  return ((parts[0]?.[0] ?? "") + (parts[1]?.[0] ?? "")).toUpperCase() || "?";
}

export function Topbar({ user, onLogout }: { user: AuthUser | null; onLogout: () => void }) {
  return (
    <header className="glass-strong sticky top-0 z-10 flex h-16 items-center gap-3 border-b border-border/70 px-4 md:px-6">
      <div className="relative hidden max-w-sm flex-1 sm:block">
        <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
        <Input placeholder="Search… (⌘K)" className="h-9 rounded-full pl-9" disabled />
      </div>

      <div className="flex flex-1 items-center justify-end gap-2">
        <AiStatusBadge />
        <ThemeSwitcher />

        <DropdownMenu>
          <DropdownMenuTrigger render={<Button variant="outline" size="icon" aria-label="Notifications" />}>
            <Bell />
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-64">
            <DropdownMenuGroup>
              <DropdownMenuLabel>Notifications</DropdownMenuLabel>
            </DropdownMenuGroup>
            <DropdownMenuSeparator />
            <div className="px-1.5 py-2 text-sm text-muted-foreground">
              Real, activity-derived alerts live on your{" "}
              <Link href="/dashboard" className="font-medium text-foreground underline underline-offset-2">
                Dashboard
              </Link>
              .
            </div>
          </DropdownMenuContent>
        </DropdownMenu>

        {user && (
          <DropdownMenu>
            <DropdownMenuTrigger
              render={<button type="button" className="flex items-center gap-2 rounded-full outline-none" />}
            >
              <Avatar className="size-8 ring-1 ring-border">
                <AvatarFallback className="bg-gradient-brand text-xs font-semibold text-primary-foreground">
                  {initials(user.displayName || user.email)}
                </AvatarFallback>
              </Avatar>
              <span className="hidden text-sm text-muted-foreground sm:inline">{user.email}</span>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-56">
              <DropdownMenuGroup>
                <DropdownMenuLabel className="truncate">{user.displayName || user.email}</DropdownMenuLabel>
              </DropdownMenuGroup>
              <DropdownMenuSeparator />
              <DropdownMenuItem variant="destructive" onClick={onLogout}>
                <LogOut />
                Sign out
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        )}
      </div>
    </header>
  );
}
