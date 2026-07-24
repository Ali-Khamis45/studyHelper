"use client";

import { useState, useSyncExternalStore } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { motion } from "framer-motion";
import {
  BarChart3,
  Bot,
  CalendarClock,
  LayoutDashboard,
  ListChecks,
  PanelLeftClose,
  PanelLeftOpen,
  Route,
  Sparkles,
  Target,
} from "lucide-react";

import { cn } from "@/lib/utils";

const NAV = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/roadmap", label: "Roadmap", icon: Route },
  { href: "/goals", label: "Goals", icon: Target },
  { href: "/planner", label: "Planner", icon: CalendarClock },
  { href: "/mentor", label: "Mentor", icon: Bot },
  { href: "/quiz", label: "Quiz", icon: ListChecks },
  { href: "/analytics", label: "Analytics", icon: BarChart3 },
] as const;

const STORAGE_KEY = "aistudyos.sidebar.collapsed";
const emptySubscribe = () => () => {};

export function Sidebar() {
  const pathname = usePathname();

  // Read the persisted preference via useSyncExternalStore (server snapshot: false) instead of an
  // effect + setState, so hydration resolves it without a post-mount state update.
  const storedCollapsed = useSyncExternalStore(
    emptySubscribe,
    () => window.localStorage.getItem(STORAGE_KEY) === "1",
    () => false,
  );
  const [override, setOverride] = useState<boolean | null>(null);
  const collapsed = override ?? storedCollapsed;

  const toggle = () => {
    const next = !collapsed;
    window.localStorage.setItem(STORAGE_KEY, next ? "1" : "0");
    setOverride(next);
  };

  return (
    <aside
      className={cn(
        "sticky top-0 z-20 hidden h-screen shrink-0 flex-col border-r border-sidebar-border bg-sidebar backdrop-blur-xl transition-[width] duration-300 md:flex",
        collapsed ? "w-[76px]" : "w-64",
      )}
    >
      <div className={cn("flex h-16 items-center gap-2 px-5", collapsed && "justify-center px-0")}>
        <span className="flex size-8 shrink-0 items-center justify-center rounded-xl bg-gradient-brand text-xs font-bold text-primary-foreground shadow-glow-primary">
          <Sparkles className="size-4" />
        </span>
        {!collapsed && (
          <span className="text-sm font-semibold tracking-tight text-sidebar-foreground">
            Study OS
          </span>
        )}
      </div>

      <nav className="flex flex-1 flex-col gap-1 px-3 py-2">
        {NAV.map(({ href, label, icon: Icon }) => {
          const active = pathname === href || pathname.startsWith(`${href}/`);
          return (
            <Link
              key={href}
              href={href}
              className={cn(
                "group relative flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium text-sidebar-foreground/70 transition-colors hover:text-sidebar-foreground",
                collapsed && "justify-center px-0",
              )}
              title={collapsed ? label : undefined}
            >
              {active && (
                <motion.span
                  layoutId="sidebar-active-pill"
                  className="absolute inset-0 rounded-xl bg-gradient-brand shadow-glow-primary"
                  transition={{ type: "spring", stiffness: 400, damping: 32 }}
                />
              )}
              <Icon className={cn("relative z-10 size-4.5 shrink-0", active && "text-primary-foreground")} />
              {!collapsed && <span className={cn("relative z-10", active && "text-primary-foreground")}>{label}</span>}
            </Link>
          );
        })}
      </nav>

      <div className="border-t border-sidebar-border p-3">
        <button
          type="button"
          onClick={toggle}
          className={cn(
            "flex w-full items-center gap-2 rounded-xl px-3 py-2 text-xs font-medium text-sidebar-foreground/60 transition-colors hover:bg-sidebar-accent/60 hover:text-sidebar-foreground",
            collapsed && "justify-center px-0",
          )}
        >
          {collapsed ? <PanelLeftOpen className="size-4" /> : <PanelLeftClose className="size-4" />}
          {!collapsed && "Collapse"}
        </button>
      </div>
    </aside>
  );
}
