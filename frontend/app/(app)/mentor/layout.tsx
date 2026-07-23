import { ConversationSidebar } from "@/components/mentor/ConversationSidebar";

export default function MentorLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex h-[calc(100vh-9rem)] min-h-[32rem] gap-4">
      <ConversationSidebar />
      <div className="flex-1 overflow-hidden">{children}</div>
    </div>
  );
}
