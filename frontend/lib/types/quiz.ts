export type QuizTypeValue = "Standard" | "Adaptive" | "Review";
export type QuestionTypeValue = "MultipleChoice" | "TrueFalse" | "ShortAnswer" | "FillBlank";
export type DifficultyValue = "Easy" | "Medium" | "Hard";
export type AttemptStatusValue = "InProgress" | "Completed";

export interface Question {
  id: string;
  order: number;
  type: QuestionTypeValue;
  topic: string;
  difficulty: DifficultyValue;
  text: string;
  options: string[] | null;
}

export interface QuizDetail {
  id: string;
  title: string;
  topic: string;
  difficulty: DifficultyValue;
  quizType: QuizTypeValue;
  questionCount: number;
  createdAtUtc: string;
  questions: Question[];
}

export interface QuizSummary {
  id: string;
  title: string;
  topic: string;
  difficulty: DifficultyValue;
  quizType: QuizTypeValue;
  questionCount: number;
  createdAtUtc: string;
  latestAttemptScore: number | null;
  latestAttemptAtUtc: string | null;
}

export interface AnswerResult {
  questionId: string;
  questionText: string;
  topic: string;
  userAnswer: string;
  correctAnswer: string;
  isCorrect: boolean;
  explanation: string;
}

export interface QuizAttemptResult {
  attemptId: string;
  quizId: string;
  quizTitle: string;
  score: number;
  correctCount: number;
  totalCount: number;
  completedAtUtc: string;
  answers: AnswerResult[];
  weakTopics: string[];
  recommendedTopics: string[];
  estimatedMasteryDelta: number;
  confidence: number;
}

export interface QuizHistoryItem {
  attemptId: string;
  quizId: string;
  quizTitle: string;
  topic: string;
  score: number | null;
  correctCount: number;
  totalCount: number;
  status: AttemptStatusValue;
  startedAtUtc: string;
  completedAtUtc: string | null;
}

export interface TopicMastery {
  topic: string;
  masteryScore: number;
  attemptsCount: number;
  lastUpdatedUtc: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasMore: boolean;
}

export interface GenerateQuizInput {
  topic: string | null;
  goalId: string | null;
  difficulty: DifficultyValue;
  questionTypes: QuestionTypeValue[];
  questionCount: number;
  quizType: QuizTypeValue;
}

export interface SubmittedAnswer {
  questionId: string;
  answer: string;
}

export type QuizGenerationStreamEvent =
  | { type: "delta"; content: string }
  | { type: "complete"; quiz: QuizDetail }
  | { type: "error"; message: string };
