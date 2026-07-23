"use client";

import { Component, type ReactNode } from "react";
import { AlertCircle } from "lucide-react";

import { Card, CardContent } from "@/components/ui/card";

/// A render-time crash in one widget (as opposed to a query error, which TanStack Query already
/// surfaces as `isError` without throwing) must not take down the rest of the Dashboard — each
/// widget gets its own boundary so a bug in one card degrades to a small inline message, not a
/// blank page. React error boundaries can only be class components; there's no hook equivalent.
export class WidgetErrorBoundary extends Component<{ label: string; children: ReactNode }, { hasError: boolean }> {
  constructor(props: { label: string; children: ReactNode }) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError() {
    return { hasError: true };
  }

  render() {
    if (this.state.hasError) {
      return (
        <Card>
          <CardContent className="flex items-center gap-2 py-6 text-sm text-muted-foreground">
            <AlertCircle className="size-4 shrink-0" />
            Couldn&apos;t load {this.props.label}.
          </CardContent>
        </Card>
      );
    }

    return this.props.children;
  }
}
