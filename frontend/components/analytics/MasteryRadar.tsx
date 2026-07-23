"use client";

import { PolarAngleAxis, PolarGrid, PolarRadiusAxis, Radar, RadarChart, ResponsiveContainer, Tooltip } from "recharts";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import type { RadarAxis as RadarAxisPoint } from "@/lib/types/analytics";

export function MasteryRadar({ data }: { data: RadarAxisPoint[] }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Mastery by Topic</CardTitle>
        <CardDescription>Top topics by activity</CardDescription>
      </CardHeader>
      <CardContent>
        {data.length === 0 ? (
          <p className="py-8 text-center text-sm text-muted-foreground">Take a quiz to see your mastery profile.</p>
        ) : (
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <RadarChart data={data}>
                <PolarGrid strokeOpacity={0.2} />
                <PolarAngleAxis dataKey="axis" tick={{ fontSize: 11 }} />
                <PolarRadiusAxis angle={30} domain={[0, 100]} tick={{ fontSize: 10 }} />
                <Radar dataKey="value" stroke="var(--primary)" fill="var(--primary)" fillOpacity={0.35} />
                <Tooltip
                  formatter={(value) => [`${value}%`, "Mastery"]}
                  contentStyle={{ borderRadius: 8, fontSize: 12, background: "var(--popover)", color: "var(--popover-foreground)", border: "1px solid var(--border)" }}
                />
              </RadarChart>
            </ResponsiveContainer>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
