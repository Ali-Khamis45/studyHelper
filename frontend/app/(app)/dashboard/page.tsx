"use client";

import Link from "next/link";
import { ChevronRight } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { appModules } from "@/lib/modules";
import { useAuthStore } from "@/lib/stores/authStore";

export default function DashboardPage() {
  const user = useAuthStore((state) => state.user);
  const firstName = user?.displayName?.trim().split(/\s+/)[0];

  return (
    <div className="mx-auto flex max-w-5xl flex-col gap-8">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">
          Welcome{firstName ? `, ${firstName}` : ""}.
        </h1>
        <p className="text-muted-foreground">
          You&apos;re signed in and the account system is fully wired up. The rest of the
          experience ships module by module — open any card below to see what&apos;s coming.
        </p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {appModules.map(({ key, title, description, icon: Icon, milestone, route }) => (
          <Link key={key} href={route} className="group block">
            <Card className="h-full cursor-pointer gap-3 py-5 transition-all duration-200 ease-out hover:-translate-y-1 hover:border-primary/50 hover:shadow-lg hover:shadow-primary/5">
              <CardHeader>
                <div className="flex items-center justify-between">
                  <span className="flex size-9 items-center justify-center rounded-lg bg-accent text-accent-foreground transition-colors group-hover:bg-primary group-hover:text-primary-foreground">
                    <Icon className="size-4.5" />
                  </span>
                  <Badge variant="secondary">{milestone}</Badge>
                </div>
                <CardTitle className="flex items-center gap-1 pt-2 text-base">
                  {title}
                  <ChevronRight className="size-4 text-muted-foreground opacity-0 transition-all -translate-x-1 group-hover:translate-x-0 group-hover:opacity-100" />
                </CardTitle>
                <CardDescription>{description}</CardDescription>
              </CardHeader>
              <CardContent />
            </Card>
          </Link>
        ))}
      </div>
    </div>
  );
}
