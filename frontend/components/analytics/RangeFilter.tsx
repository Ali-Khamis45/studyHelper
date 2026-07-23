"use client";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import type { AnalyticsRangePreset } from "@/lib/types/analytics";

const PRESETS: { value: AnalyticsRangePreset; label: string }[] = [
  { value: "today", label: "Today" },
  { value: "week", label: "Week" },
  { value: "month", label: "Month" },
  { value: "year", label: "Year" },
  { value: "custom", label: "Custom" },
];

export function RangeFilter({
  preset,
  onPresetChange,
  customFrom,
  customTo,
  onCustomChange,
}: {
  preset: AnalyticsRangePreset;
  onPresetChange: (preset: AnalyticsRangePreset) => void;
  customFrom: string;
  customTo: string;
  onCustomChange: (from: string, to: string) => void;
}) {
  return (
    <div className="flex flex-wrap items-center gap-2">
      <div className="flex items-center gap-1 rounded-full border bg-muted/40 p-1">
        {PRESETS.map(({ value, label }) => (
          <Button
            key={value}
            type="button"
            size="sm"
            variant={preset === value ? "default" : "ghost"}
            className={cn("rounded-full px-3", preset !== value && "text-muted-foreground")}
            onClick={() => onPresetChange(value)}
          >
            {label}
          </Button>
        ))}
      </div>

      {preset === "custom" && (
        <div className="flex items-center gap-2">
          <Input type="date" value={customFrom} onChange={(e) => onCustomChange(e.target.value, customTo)} className="w-auto" />
          <span className="text-sm text-muted-foreground">to</span>
          <Input type="date" value={customTo} onChange={(e) => onCustomChange(customFrom, e.target.value)} className="w-auto" />
        </div>
      )}
    </div>
  );
}
