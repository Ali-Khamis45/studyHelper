"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { ArrowLeft, ArrowRight, Sparkles } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { useStreamGenerateRoadmap } from "@/lib/hooks/useRoadmap";
import { cn } from "@/lib/utils";

const CAREER_SUGGESTIONS = [
  "Backend Developer",
  "Frontend Developer",
  "Full Stack Developer",
  "AI Engineer",
  "Machine Learning Engineer",
  "Data Scientist",
  "Cyber Security Engineer",
  "Mobile Developer",
  "DevOps Engineer",
  "Cloud Engineer",
  "UI/UX Designer",
];

const EXPERIENCE_LEVELS = ["Complete beginner", "Some experience", "Intermediate", "Experienced"];
const LEARNING_STYLES = ["Reading", "Video", "Hands-on projects", "Mixed"];

const STEPS = ["Goal", "Experience", "Study style", "Generate"] as const;

export function CreateRoadmapWizard({ open, onOpenChange }: { open: boolean; onOpenChange: (open: boolean) => void }) {
  const router = useRouter();
  const stream = useStreamGenerateRoadmap();

  const [step, setStep] = useState(0);
  const [careerGoal, setCareerGoal] = useState("");
  const [currentExperience, setCurrentExperience] = useState(EXPERIENCE_LEVELS[0]);
  const [existingSkills, setExistingSkills] = useState("");
  const [hoursPerWeek, setHoursPerWeek] = useState(10);
  const [learningStyle, setLearningStyle] = useState(LEARNING_STYLES[3]);
  const [targetCompletionDate, setTargetCompletionDate] = useState("");
  const [preferredLanguage, setPreferredLanguage] = useState("");
  const [preferredResources, setPreferredResources] = useState("");

  useEffect(() => {
    if (stream.roadmap) {
      onOpenChange(false);
      router.push(`/roadmap/${stream.roadmap.id}`);
    }
  }, [stream.roadmap, onOpenChange, router]);

  const canProceed = step !== 0 || careerGoal.trim().length > 0;

  // Resets local form state on close — a real event handler, not an effect, so this only ever
  // runs in direct response to the user (or the auto-close above) closing the dialog.
  const handleOpenChange = (next: boolean) => {
    if (stream.isStreaming) return;
    if (!next) {
      setStep(0);
      setCareerGoal("");
      setExistingSkills("");
      setPreferredLanguage("");
      setPreferredResources("");
      setTargetCompletionDate("");
    }
    onOpenChange(next);
  };

  const handleGenerate = () => {
    stream.start({
      careerGoal: careerGoal.trim(),
      currentExperience,
      existingSkills: existingSkills.trim() || null,
      hoursPerWeek,
      learningStyle,
      targetCompletionDate: targetCompletionDate || null,
      preferredLanguage: preferredLanguage.trim() || null,
      preferredResources: preferredResources.trim() || null,
    });
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Create New Learning Journey</DialogTitle>
          <DialogDescription>A guided intake, then the AI designs a personalized roadmap around it.</DialogDescription>
        </DialogHeader>

        <div className="flex items-center gap-1.5">
          {STEPS.map((label, i) => (
            <div key={label} className="flex flex-1 items-center gap-1.5">
              <span className={cn("h-1.5 flex-1 rounded-full transition-colors", i <= step ? "bg-gradient-brand" : "bg-muted")} />
            </div>
          ))}
        </div>

        <div className="flex min-h-64 flex-col gap-4">
          {step === 0 && (
            <div className="flex flex-col gap-3">
              <div className="flex flex-col gap-1">
                <h3 className="text-base font-semibold">What do you want to become?</h3>
                <p className="text-sm text-muted-foreground">Pick a suggestion or type your own career goal.</p>
              </div>
              <Input
                autoFocus
                placeholder="e.g. Backend .NET Developer"
                value={careerGoal}
                onChange={(e) => setCareerGoal(e.target.value)}
              />
              <div className="flex flex-wrap gap-1.5">
                {CAREER_SUGGESTIONS.map((suggestion) => (
                  <button
                    key={suggestion}
                    type="button"
                    onClick={() => setCareerGoal(suggestion)}
                    className={cn(
                      "rounded-full border px-3 py-1 text-xs font-medium transition-colors",
                      careerGoal === suggestion
                        ? "border-primary bg-primary text-primary-foreground"
                        : "border-border bg-background text-muted-foreground hover:bg-muted",
                    )}
                  >
                    {suggestion}
                  </button>
                ))}
              </div>
            </div>
          )}

          {step === 1 && (
            <div className="flex flex-col gap-4">
              <div className="flex flex-col gap-1">
                <h3 className="text-base font-semibold">Where are you starting from?</h3>
                <p className="text-sm text-muted-foreground">This shapes pacing and depth, not just content.</p>
              </div>
              <div className="flex flex-col gap-2">
                <Label>Current experience</Label>
                <Select value={currentExperience} onValueChange={(v) => v && setCurrentExperience(v)}>
                  <SelectTrigger className="w-full">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {EXPERIENCE_LEVELS.map((level) => (
                      <SelectItem key={level} value={level}>
                        {level}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="flex flex-col gap-2">
                <Label htmlFor="skills">Existing skills (optional)</Label>
                <Textarea
                  id="skills"
                  rows={3}
                  placeholder="e.g. basic HTML/CSS, a bit of Python from a course"
                  value={existingSkills}
                  onChange={(e) => setExistingSkills(e.target.value)}
                />
              </div>
            </div>
          )}

          {step === 2 && (
            <div className="flex flex-col gap-4">
              <div className="flex flex-col gap-1">
                <h3 className="text-base font-semibold">How do you like to study?</h3>
                <p className="text-sm text-muted-foreground">All optional — the AI uses sensible defaults otherwise.</p>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="flex flex-col gap-2">
                  <Label htmlFor="hours">Hours per week</Label>
                  <Input
                    id="hours"
                    type="number"
                    min={1}
                    max={80}
                    value={hoursPerWeek}
                    onChange={(e) => setHoursPerWeek(Math.min(80, Math.max(1, Number(e.target.value) || 1)))}
                  />
                </div>
                <div className="flex flex-col gap-2">
                  <Label>Learning style</Label>
                  <Select value={learningStyle} onValueChange={(v) => v && setLearningStyle(v)}>
                    <SelectTrigger className="w-full">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {LEARNING_STYLES.map((style) => (
                        <SelectItem key={style} value={style}>
                          {style}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="flex flex-col gap-2">
                  <Label htmlFor="language">Preferred language (optional)</Label>
                  <Input id="language" placeholder="e.g. C#" value={preferredLanguage} onChange={(e) => setPreferredLanguage(e.target.value)} />
                </div>
                <div className="flex flex-col gap-2">
                  <Label htmlFor="targetDate">Target date (optional)</Label>
                  <Input id="targetDate" type="date" value={targetCompletionDate} onChange={(e) => setTargetCompletionDate(e.target.value)} />
                </div>
              </div>
              <div className="flex flex-col gap-2">
                <Label htmlFor="resources">Preferred resources (optional)</Label>
                <Input
                  id="resources"
                  placeholder="e.g. official docs, YouTube, freeCodeCamp"
                  value={preferredResources}
                  onChange={(e) => setPreferredResources(e.target.value)}
                />
              </div>
            </div>
          )}

          {step === 3 && (
            <div className="flex flex-col gap-3">
              {!stream.isStreaming && !stream.error && (
                <div className="flex flex-col gap-3">
                  <div className="flex flex-col gap-1">
                    <h3 className="text-base font-semibold">Ready to build your roadmap</h3>
                    <p className="text-sm text-muted-foreground">
                      <span className="font-medium text-foreground">{careerGoal}</span> &middot; {currentExperience} &middot; {hoursPerWeek}h/week &middot; {learningStyle}
                    </p>
                  </div>
                  <Button onClick={handleGenerate} size="lg">
                    <Sparkles /> Generate My Roadmap
                  </Button>
                </div>
              )}

              {stream.isStreaming && (
                <div className="flex flex-col gap-3">
                  <div className="flex items-center gap-2 text-sm font-medium">
                    <Sparkles className="size-4 animate-pulse text-primary" /> Designing your roadmap&hellip;
                  </div>
                  <pre className="max-h-48 overflow-y-auto rounded-md bg-muted p-3 text-xs whitespace-pre-wrap text-muted-foreground">
                    {stream.partialText || "Connecting to your AI mentor…"}
                  </pre>
                </div>
              )}

              {stream.error && (
                <div className="flex flex-col gap-3">
                  <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{stream.error}</p>
                  <Button variant="outline" onClick={handleGenerate}>
                    Try again
                  </Button>
                </div>
              )}
            </div>
          )}
        </div>

        <DialogFooter className="flex items-center justify-between sm:justify-between">
          <Button variant="ghost" disabled={step === 0 || stream.isStreaming} onClick={() => setStep((s) => s - 1)}>
            <ArrowLeft /> Back
          </Button>
          {step < STEPS.length - 1 && (
            <Button disabled={!canProceed} onClick={() => setStep((s) => s + 1)}>
              Next <ArrowRight />
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
