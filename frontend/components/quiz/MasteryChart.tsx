"use client";

import { Bar, BarChart, CartesianGrid, Cell, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import type { TopicMastery } from "@/lib/types/quiz";

function colorFor(score: number) {
  if (score >= 0.8) return "#22c55e";
  if (score >= 0.6) return "#84cc16";
  if (score >= 0.4) return "#f59e0b";
  return "#ef4444";
}

export function MasteryChart({ mastery }: { mastery: TopicMastery[] }) {
  if (mastery.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Topic mastery</CardTitle>
          <CardDescription>Take a quiz to start building your mastery profile.</CardDescription>
        </CardHeader>
      </Card>
    );
  }

  const data = [...mastery]
    .sort((a, b) => b.masteryScore - a.masteryScore)
    .map((m) => ({ topic: m.topic, score: Math.round(m.masteryScore * 100) }));

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Topic mastery</CardTitle>
        <CardDescription>Weighted moving average across your quiz attempts, by topic.</CardDescription>
      </CardHeader>
      <CardContent>
        <div style={{ height: Math.max(200, data.length * 36) }}>
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={data} layout="vertical" margin={{ left: 8, right: 16 }}>
              <CartesianGrid horizontal={false} strokeOpacity={0.15} />
              <XAxis type="number" domain={[0, 100]} tick={{ fontSize: 12 }} tickFormatter={(v: number) => `${v}%`} />
              <YAxis type="category" dataKey="topic" width={120} tick={{ fontSize: 12 }} />
              <Tooltip
                formatter={(value) => [`${value}%`, "Mastery"]}
                contentStyle={{ borderRadius: 8, fontSize: 12, background: "var(--popover)", color: "var(--popover-foreground)", border: "1px solid var(--border)" }}
              />
              <Bar dataKey="score" radius={[0, 6, 6, 0]}>
                {data.map((entry) => (
                  <Cell key={entry.topic} fill={colorFor(entry.score / 100)} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>
      </CardContent>
    </Card>
  );
}
