"use client";

import { Download, FileText } from "lucide-react";

import { Button } from "@/components/ui/button";
import { useExportAnalytics } from "@/lib/hooks/useAnalytics";

export function ExportButtons({ range }: { range?: { from?: string; to?: string } }) {
  const exportAnalytics = useExportAnalytics();

  return (
    <div className="flex items-center gap-2">
      <Button variant="outline" size="sm" onClick={() => exportAnalytics.mutate({ format: "pdf", range })} disabled={exportAnalytics.isPending}>
        <FileText />
        PDF
      </Button>
      <Button variant="outline" size="sm" onClick={() => exportAnalytics.mutate({ format: "csv", range })} disabled={exportAnalytics.isPending}>
        <Download />
        CSV
      </Button>
    </div>
  );
}
