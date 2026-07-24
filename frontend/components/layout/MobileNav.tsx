"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { BarChart3, Bot, CalendarClock, LayoutDashboard, ListChecks, Route, Target } from "lucide-react";

import { cn } from "@/lib/utils";

const NAV = [
  { href: "/dashboard", label: "Home", icon: LayoutDashboard },
  { href: "/roadmap", label: "Roadmap", icon: Route },
  { href: "/goals", label: "Goals", icon: Target },
  { href: "/planner", label: "Planner", icon: CalendarClock },
  { href: "/mentor", label: "Mentor", icon: Bot },
  { href: "/quiz", label: "Quiz", icon: ListChecks },
  { href: "/analytics", label: "Stats", icon: BarChart3 },
] as const;

export function MobileNav() {
  const pathname = usePathname();

  return (
    <nav className="glass-strong sticky bottom-0 z-20 flex items-center justify-around border-t border-border/70 px-1 py-2 md:hidden">
      {NAV.map(({ href, label, icon: Icon }) => {
        const active = pathname === href || pathname.startsWith(`${href}/`);
        return (
          <Link
            key={href}
            href={href}
            className={cn(
              "flex flex-col items-center gap-0.5 rounded-lg px-2.5 py-1 text-[0.65rem] font-medium text-muted-foreground transition-colors",
              active && "text-primary",
            )}
          >
            <Icon className={cn("size-5", active && "drop-shadow-[0_0_6px_oklch(0.56_0.22_274_/_45%)]")} />
            {label}
          </Link>
        );
      })}
    </nav>
  );
}
