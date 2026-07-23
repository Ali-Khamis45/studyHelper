import Link from "next/link";
import { CalendarPlus, ListChecks, MessageCirclePlus, Target } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { buttonVariants } from "@/components/ui/button";

const ACTIONS = [
  { href: "/goals", label: "New Goal", icon: Target },
  { href: "/planner", label: "Plan Today", icon: CalendarPlus },
  { href: "/mentor", label: "Ask Mentor", icon: MessageCirclePlus },
  { href: "/quiz", label: "Take a Quiz", icon: ListChecks },
];

export function QuickActionsWidget() {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Quick Actions</CardTitle>
      </CardHeader>
      <CardContent className="grid grid-cols-2 gap-2">
        {ACTIONS.map(({ href, label, icon: Icon }) => (
          <Link key={href} href={href} className={buttonVariants({ variant: "outline", className: "flex-col gap-1.5 py-4 h-auto" })}>
            <Icon className="size-4" />
            <span className="text-xs">{label}</span>
          </Link>
        ))}
      </CardContent>
    </Card>
  );
}
