"use client"

import { useTheme } from "next-themes"
import { Toaster as Sonner, type ToasterProps } from "sonner"
import { CircleCheckIcon, InfoIcon, TriangleAlertIcon, OctagonXIcon, Loader2Icon } from "lucide-react"

const DARK_LIKE_THEMES = new Set(["dark", "midnight"]);

const Toaster = ({ ...props }: ToasterProps) => {
  const { resolvedTheme } = useTheme()
  // Custom themes (midnight, focus) aren't in Sonner's own light/dark/system union — map each to
  // whichever toast palette actually matches its background so toasts stay readable.
  const toastTheme: ToasterProps["theme"] = resolvedTheme
    ? DARK_LIKE_THEMES.has(resolvedTheme)
      ? "dark"
      : "light"
    : "system"

  return (
    <Sonner
      theme={toastTheme}
      className="toaster group"
      icons={{
        success: (
          <CircleCheckIcon className="size-4" />
        ),
        info: (
          <InfoIcon className="size-4" />
        ),
        warning: (
          <TriangleAlertIcon className="size-4" />
        ),
        error: (
          <OctagonXIcon className="size-4" />
        ),
        loading: (
          <Loader2Icon className="size-4 animate-spin" />
        ),
      }}
      style={
        {
          "--normal-bg": "var(--popover)",
          "--normal-text": "var(--popover-foreground)",
          "--normal-border": "var(--border)",
          "--border-radius": "var(--radius)",
        } as React.CSSProperties
      }
      toastOptions={{
        classNames: {
          toast: "cn-toast",
        },
      }}
      {...props}
    />
  )
}

export { Toaster }
