"use client";

import { useEffect, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";

const QUOTES = [
  "Small progress every day becomes big success.",
  "Discipline beats motivation.",
  "Today's effort is tomorrow's achievement.",
] as const;

export function QuoteRotator() {
  const [index, setIndex] = useState(0);

  useEffect(() => {
    const id = setInterval(() => setIndex((i) => (i + 1) % QUOTES.length), 5000);
    return () => clearInterval(id);
  }, []);

  return (
    <div className="relative h-24 sm:h-20">
      <AnimatePresence mode="wait">
        <motion.blockquote
          key={index}
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          exit={{ opacity: 0, y: -10 }}
          transition={{ duration: 0.4, ease: "easeOut" }}
          className="absolute inset-0 text-2xl leading-snug font-medium text-balance"
        >
          {QUOTES[index]}
        </motion.blockquote>
      </AnimatePresence>
    </div>
  );
}
