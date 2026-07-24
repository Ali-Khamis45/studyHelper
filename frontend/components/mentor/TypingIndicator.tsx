export function TypingIndicator({ label = "Mentor is thinking" }: { label?: string }) {
  return (
    <div className="flex items-center gap-2 px-1 text-xs text-muted-foreground">
      <span className="glass ring-glass flex items-center gap-1 rounded-2xl rounded-bl-md px-3.5 py-2.5">
        <span className="size-1.5 animate-bounce rounded-full bg-gradient-brand [animation-delay:-0.3s]" />
        <span className="size-1.5 animate-bounce rounded-full bg-gradient-brand [animation-delay:-0.15s]" />
        <span className="size-1.5 animate-bounce rounded-full bg-gradient-brand" />
      </span>
      <span>{label}</span>
    </div>
  );
}
