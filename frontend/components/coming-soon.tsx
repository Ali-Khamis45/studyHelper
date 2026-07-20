import Link from "next/link";
import { notFound } from "next/navigation";
import { ArrowLeft, Check, Clock } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { buttonVariants } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { getModule } from "@/lib/modules";

export function ComingSoon({ moduleKey }: { moduleKey: string }) {
  const appModule = getModule(moduleKey);
  if (!appModule) notFound();

  const { title, milestone, description, status, capabilities, icon: Icon } = appModule;

  return (
    <div className="mx-auto flex max-w-2xl flex-col items-center gap-6 py-8 text-center">
      <span className="flex size-16 items-center justify-center rounded-2xl bg-accent text-accent-foreground">
        <Icon className="size-7" />
      </span>

      <div className="flex flex-col items-center gap-2">
        <div className="flex items-center gap-2">
          <h1 className="text-2xl font-semibold tracking-tight">{title}</h1>
          <Badge variant="secondary">{milestone}</Badge>
        </div>
        <p className="max-w-md text-muted-foreground">{description}</p>
      </div>

      <div className="flex items-center gap-2 rounded-full bg-muted px-4 py-1.5 text-sm font-medium text-muted-foreground">
        <Clock className="size-3.5" />
        {status}
      </div>

      <Card className="w-full text-left">
        <CardHeader>
          <CardTitle className="text-base">What this will do</CardTitle>
        </CardHeader>
        <CardContent>
          <ul className="flex flex-col gap-3">
            {capabilities.map((capability) => (
              <li key={capability} className="flex items-start gap-2.5 text-sm">
                <Check className="mt-0.5 size-4 shrink-0 text-primary" />
                <span>{capability}</span>
              </li>
            ))}
          </ul>
        </CardContent>
      </Card>

      <Link href="/dashboard" className={buttonVariants({ variant: "outline" })}>
        <ArrowLeft />
        Back to Dashboard
      </Link>
    </div>
  );
}
