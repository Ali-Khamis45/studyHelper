import { FloatingIllustration } from "@/components/auth/FloatingIllustration";
import { QuoteRotator } from "@/components/auth/QuoteRotator";
import { Reveal } from "@/components/motion/Reveal";

export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="grid min-h-screen w-full lg:grid-cols-2">
      <div className="relative hidden flex-col justify-between overflow-hidden bg-gradient-brand p-10 text-primary-foreground lg:flex">
        <div
          className="pointer-events-none absolute inset-0 opacity-50"
          style={{
            backgroundImage:
              "radial-gradient(circle at 15% 15%, color-mix(in oklch, var(--primary-foreground) 25%, transparent) 0%, transparent 45%), radial-gradient(circle at 85% 85%, color-mix(in oklch, var(--brand-cyan) 35%, transparent) 0%, transparent 55%)",
          }}
        />
        <FloatingIllustration />

        <div className="relative flex items-center gap-2 text-lg font-semibold tracking-tight">
          <span className="flex size-8 items-center justify-center rounded-lg bg-primary-foreground/15 font-bold backdrop-blur">
            AI
          </span>
          Study OS
        </div>

        <div className="relative flex flex-col gap-4">
          <QuoteRotator />
          <p className="text-sm text-primary-foreground/70">
            Set a goal, and the mentor breaks it down into milestones, weekly
            objectives, and a next action — every day.
          </p>
        </div>
      </div>

      <div className="relative flex items-center justify-center overflow-hidden p-6 sm:p-10">
        <div className="pointer-events-none absolute inset-0 bg-gradient-brand-soft" />
        <Reveal className="relative w-full max-w-sm">
          <div className="mb-8 flex items-center gap-2 text-lg font-semibold tracking-tight lg:hidden">
            <span className="flex size-8 items-center justify-center rounded-lg bg-gradient-brand text-primary-foreground font-bold">
              AI
            </span>
            Study OS
          </div>

          <div className="glass ring-glass rounded-3xl border border-border/60 p-6 sm:p-8">{children}</div>
        </Reveal>
      </div>
    </div>
  );
}
