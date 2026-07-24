export type RoadmapDifficultyValue = "Beginner" | "Intermediate" | "Advanced";
export type RoadmapStatusValue = "Active" | "Completed" | "Archived";
export type TopicStatusValue = "Locked" | "Available" | "InProgress" | "Completed" | "Mastered";

export interface RoadmapResource {
  type: string;
  title: string;
  url: string | null;
}

export interface PrerequisiteStatus {
  topicId: string;
  title: string;
  met: boolean;
}

export interface RoadmapTopicNode {
  id: string;
  parentTopicId: string | null;
  order: number;
  title: string;
  description: string;
  estimatedHours: number;
  difficulty: RoadmapDifficultyValue;
  resources: RoadmapResource[];
  suggestedProjects: string[];
  prerequisites: PrerequisiteStatus[];
  linkedMasteryTopic: string;
  masteryScore: number;
  status: TopicStatusValue;
  manuallyCompleted: boolean;
  notes: string | null;
  updatedAtUtc: string;
  children: RoadmapTopicNode[];
}

export interface Roadmap {
  id: string;
  careerGoal: string;
  title: string;
  description: string;
  difficulty: RoadmapDifficultyValue;
  estimatedWeeks: number;
  status: RoadmapStatusValue;
  progressPercent: number;
  completedTopicCount: number;
  totalTopicCount: number;
  totalEstimatedHours: number;
  remainingEstimatedHours: number;
  createdAtUtc: string;
  updatedAtUtc: string;
  sections: RoadmapTopicNode[];
}

export interface RoadmapSummary {
  id: string;
  careerGoal: string;
  title: string;
  difficulty: RoadmapDifficultyValue;
  estimatedWeeks: number;
  status: RoadmapStatusValue;
  progressPercent: number;
  completedTopicCount: number;
  totalTopicCount: number;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface GenerateRoadmapInput {
  careerGoal: string;
  currentExperience: string | null;
  existingSkills: string | null;
  hoursPerWeek: number | null;
  learningStyle: string | null;
  targetCompletionDate: string | null;
  preferredLanguage: string | null;
  preferredResources: string | null;
}

export type RoadmapGenerationStreamEvent =
  | { type: "delta"; content: string }
  | { type: "complete"; roadmap: Roadmap }
  | { type: "error"; message: string };
