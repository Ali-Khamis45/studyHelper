import { Card, CardContent } from "@/components/ui/card";
import { cn } from "@/lib/utils";

export function StatCard({ label, value, suffix, className }: { label: string; value: string | number; suffix?: string; className?: string }) {
  return (
    <Card className={cn("gap-1 py-4", className)}>
      <CardContent className="flex flex-col gap-1 px-4">
        <span className="text-xs font-medium text-muted-foreground">{label}</span>
        <span className="text-2xl font-bold tabular-nums">
          {value}
          {suffix && <span className="ml-0.5 text-base font-medium text-muted-foreground">{suffix}</span>}
        </span>
      </CardContent>
    </Card>
  );
}
