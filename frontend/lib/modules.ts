import { BarChart3, Calendar, ListChecks, MessageCircle, Target, type LucideIcon } from "lucide-react";

export interface AppModule {
  key: string;
  title: string;
  route: string;
  milestone: string;
  description: string;
  icon: LucideIcon;
  status: string;
  capabilities: string[];
}

export const appModules: AppModule[] = [
  {
    key: "goals",
    title: "Goals",
    route: "/goals",
    milestone: "M3",
    description:
      "Turn a long-term goal into a concrete plan — milestones, weekly objectives, and daily tasks, generated with AI.",
    icon: Target,
    status: "Not started yet",
    capabilities: [
      "Create, edit, and archive goals",
      "AI-powered decomposition into milestones and weekly objectives",
      "Manual editing of any AI-generated step",
      "Progress tracking across the full goal tree",
    ],
  },
  {
    key: "planner",
    title: "Study Planner",
    route: "/planner",
    milestone: "M5–M6",
    description:
      "A daily loop built from your active goals — what to do today, and why, regenerated as your progress changes.",
    icon: Calendar,
    status: "Not started yet",
    capabilities: [
      "Daily task list generated from your goals",
      "AI recommendation with reasoning for today's focus",
      "Mark tasks complete, skip, or reschedule",
      "Weekly view across all active goals",
    ],
  },
  {
    key: "mentor",
    title: "AI Mentor",
    route: "/mentor",
    milestone: "M7",
    description:
      "A conversational mentor that knows your goals and history, and routes each message to the right specialist agent.",
    icon: MessageCircle,
    status: "Not started yet",
    capabilities: [
      "Persistent chat history per conversation",
      "Automatic routing to Planner, Tutor, Examiner, or general mentor",
      "Context-aware answers grounded in your actual goals and progress",
      "Streamed responses in real time",
    ],
  },
  {
    key: "quiz",
    title: "Quiz Engine",
    route: "/quiz",
    milestone: "M8",
    description:
      "AI-generated quizzes tied to your goals, with instant grading and feedback on short-answer responses.",
    icon: ListChecks,
    status: "Not started yet",
    capabilities: [
      "Auto-generated multiple-choice, true/false, and short-answer quizzes",
      "Quizzes scoped to a specific goal or weekly objective",
      "Instant scoring with AI-graded feedback",
      "Attempt history and accuracy trends",
    ],
  },
  {
    key: "analytics",
    title: "Analytics",
    route: "/analytics",
    milestone: "M9",
    description: "A running picture of your study habits — completion rates, streaks, and quiz accuracy over time.",
    icon: BarChart3,
    status: "Not started yet",
    capabilities: [
      "Task completion rate over rolling windows",
      "Study streak tracking",
      "Quiz accuracy trends by topic",
      "Goal-level progress summaries",
    ],
  },
];

export function getModule(key: string): AppModule | undefined {
  return appModules.find((module) => module.key === key);
}
