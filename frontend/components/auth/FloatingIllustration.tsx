"use client";

import { motion } from "framer-motion";
import { Bot, BookOpen, Brain, GraduationCap, Sparkles } from "lucide-react";

const TILES = [
  { icon: BookOpen, top: "8%", left: "12%", size: "size-14", delay: 0, duration: 6 },
  { icon: Brain, top: "58%", left: "4%", size: "size-16", delay: 0.6, duration: 7 },
  { icon: Bot, top: "20%", left: "68%", size: "size-16", delay: 0.3, duration: 6.5 },
  { icon: GraduationCap, top: "68%", left: "62%", size: "size-14", delay: 0.9, duration: 5.5 },
] as const;

export function FloatingIllustration() {
  return (
    <div className="pointer-events-none absolute inset-0 hidden sm:block">
      {TILES.map(({ icon: Icon, top, left, size, delay, duration }, i) => (
        <motion.div
          key={i}
          className={`absolute ${size} flex items-center justify-center rounded-2xl border border-primary-foreground/15 bg-primary-foreground/10 backdrop-blur-xl`}
          style={{ top, left }}
          animate={{ y: [0, -14, 0] }}
          transition={{ duration, delay, repeat: Infinity, ease: "easeInOut" }}
        >
          <Icon className="size-1/2 text-primary-foreground/80" />
        </motion.div>
      ))}
      <motion.div
        className="absolute top-[42%] left-[36%] flex size-20 items-center justify-center rounded-3xl border border-primary-foreground/20 bg-primary-foreground/10 backdrop-blur-xl"
        animate={{ y: [0, 12, 0], rotate: [0, 3, 0] }}
        transition={{ duration: 8, repeat: Infinity, ease: "easeInOut" }}
      >
        <Sparkles className="size-9 text-primary-foreground/85" />
      </motion.div>
    </div>
  );
}
