"use client";

import { Check, Leaf, Moon, Sparkles, Sun } from "lucide-react";
import { useTheme } from "next-themes";

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
import { useMounted } from "@/lib/hooks/useMounted";
import { cn } from "@/lib/utils";

const THEMES = [
  { value: "light", label: "Light", description: "Clean, bright, soft blue gradients", icon: Sun },
  { value: "dark", label: "Dark", description: "Navy glass with a blue glow", icon: Moon },
  { value: "midnight", label: "Midnight", description: "Deep navy, purple glow", icon: Sparkles },
  { value: "focus", label: "Focus", description: "Muted green — easy on long sessions", icon: Leaf },
] as const;

export function ThemeSwitcher() {
  const { theme, setTheme } = useTheme();
  const mounted = useMounted();
  const ActiveIcon = THEMES.find((t) => t.value === theme)?.icon ?? Sun;

  return (
    <DropdownMenu>
      <DropdownMenuTrigger render={<Button type="button" variant="outline" size="icon" aria-label="Change theme" />}>
        {mounted ? <ActiveIcon /> : <Sun />}
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-64">
        <DropdownMenuGroup>
          <DropdownMenuLabel>Theme</DropdownMenuLabel>
        </DropdownMenuGroup>
        <DropdownMenuSeparator />
        {THEMES.map(({ value, label, description, icon: Icon }) => (
          <DropdownMenuItem key={value} onClick={() => setTheme(value)} className="items-start gap-2.5 py-2">
            <span className="mt-0.5 flex size-7 shrink-0 items-center justify-center rounded-lg bg-gradient-brand text-primary-foreground">
              <Icon className="size-3.5" />
            </span>
            <span className="flex flex-1 flex-col">
              <span className="text-sm font-medium">{label}</span>
              <span className="text-xs text-muted-foreground">{description}</span>
            </span>
            <Check className={cn("mt-1.5 size-3.5 shrink-0", theme === value ? "opacity-100" : "opacity-0")} />
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
