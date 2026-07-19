import { create } from "zustand";

interface UiState {
  sidebarOpen: boolean;
  toggleSidebar: () => void;
  mentorDraft: string;
  setMentorDraft: (draft: string) => void;
}

export const useUiStore = create<UiState>((set) => ({
  sidebarOpen: true,
  toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
  mentorDraft: "",
  setMentorDraft: (mentorDraft) => set({ mentorDraft }),
}));
